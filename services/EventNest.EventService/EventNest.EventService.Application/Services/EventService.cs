using EventNest.EventService.Application.DTOs.Events;
using EventNest.EventService.Application.Services.Interfaces;
using EventNest.EventService.Domain.Entities;
using EventNest.EventService.Domain.Enums;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.EventService.Application.Services;

public class EventService : IEventService
{
    private const string FallbackTagColor = "#6366f1";

    private readonly IEventRepository _repository;
    private readonly ITagGrpcClient _tagGrpcClient;
    private readonly IRsvpGrpcClient? _rsvpGrpcClient;

    public EventService(
        IEventRepository repository,
        ITagGrpcClient tagGrpcClient,
        IRsvpGrpcClient? rsvpGrpcClient = null)
    {
        _repository = repository;
        _tagGrpcClient = tagGrpcClient;
        _rsvpGrpcClient = rsvpGrpcClient;
    }

    public async Task<EventDto> CreateAsync(CreateEventRequestDto request, Guid organizerId, string organizerName)
    {
        if (await _repository.ExistsByTitleAsync(request.Title))
            throw new ConflictException($"Event with title '{request.Title}' already exists.");

        var evt = Event.Create(
            request.Title,
            request.Description,
            request.Location,
            request.StartsAt,
            request.EndsAt,
            request.Capacity,
            organizerId,
            organizerName);

        if (!string.IsNullOrEmpty(request.Visibility) &&
            Enum.TryParse<EventVisibility>(request.Visibility, true, out var visibility))
        {
            evt.UpdateVisibility(visibility);
        }

        await AddValidatedTagsAsync(evt, MergeTagIds(request.Tags, request.TagIds));

        await _repository.AddAsync(evt);
        await _repository.SaveChangesAsync();

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    public async Task<EventDto?> GetByIdAsync(Guid id)
    {
        var evt = await _repository.GetByIdAsync(id);
        if (evt is null)
            return null;

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return await MapManyAsync(events);
    }

    public async Task<PagedResultDto<EventDto>> GetPagedAsync(
        EventListQueryDto query, bool includeUnpublished)
    {
        ValidateQuery(query);

        var isPopularity = query.Sort?.ToLower() == "popularity";

        var (events, total) = await _repository.QueryAsync(query, includeUnpublished);
        var pages = (int)Math.Ceiling((double)total / Math.Max(query.PageSize, 1));

        if (isPopularity)
        {
            var counts = await GetCountsAsync(events.Select(e => e.Id).ToList());
            events = events
                .OrderByDescending(e => counts.TryGetValue(e.Id, out var c) ? c.Going : 0)
                .ToList();

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 1);
            events = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        var items = await MapManyAsync(events);

        return new PagedResultDto<EventDto>(items, total, query.Page, query.PageSize, pages);
    }

    public async Task<List<EventDto>> GetByOrganizerAsync(Guid organizerId)
    {
        var events = await _repository.GetByOrganizerAsync(organizerId);
        return await MapManyAsync(events);
    }

    public async Task<List<EventDto>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<EventStatus>(status, true, out var eventStatus))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { $"Invalid status '{status}'. Valid values: Draft, Published, Cancelled, Completed" }
            });

        var events = await _repository.GetByStatusAsync(eventStatus);
        return await MapManyAsync(events);
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventRequestDto request, Guid userId)
    {
        var evt = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Event with id {id} not found.");

        if (evt.OrganizerId != userId)
            throw new ForbiddenException("You can only manage your own events.");

        if (await _repository.ExistsByTitleAsync(request.Title, id))
            throw new ConflictException($"Event with title '{request.Title}' already exists.");

        evt.UpdateTitle(request.Title);
        evt.UpdateDescription(request.Description);
        evt.UpdateLocation(request.Location);
        evt.UpdateSchedule(request.StartsAt, request.EndsAt);
        evt.UpdateCapacity(request.Capacity);

        if (!string.IsNullOrEmpty(request.Visibility) &&
            Enum.TryParse<EventVisibility>(request.Visibility, true, out var visibility))
        {
            evt.UpdateVisibility(visibility);
        }

        evt.EventTags.Clear();
        await AddValidatedTagsAsync(evt, MergeTagIds(request.Tags, request.TagIds));

        await _repository.UpdateAsync(evt);
        await _repository.SaveChangesAsync();

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var evt = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Event with id {id} not found.");

        if (evt.OrganizerId != userId)
            throw new ForbiddenException("You can only manage your own events.");

        await _repository.DeleteAsync(evt);
        await _repository.SaveChangesAsync();
    }

    public async Task<EventDto> PublishAsync(Guid id, Guid userId)
    {
        var evt = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Event with id {id} not found.");

        if (evt.OrganizerId != userId)
            throw new ForbiddenException("You can only manage your own events.");

        evt.Publish();
        await _repository.UpdateAsync(evt);
        await _repository.SaveChangesAsync();

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    public async Task<EventDto> CancelAsync(Guid id, Guid userId)
    {
        var evt = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Event with id {id} not found.");

        if (evt.OrganizerId != userId)
            throw new ForbiddenException("You can only manage your own events.");

        evt.Cancel();
        await _repository.UpdateAsync(evt);
        await _repository.SaveChangesAsync();

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    public async Task<EventDto> CompleteAsync(Guid id, Guid userId)
    {
        var evt = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Event with id {id} not found.");

        if (evt.OrganizerId != userId)
            throw new ForbiddenException("You can only manage your own events.");

        evt.Complete();
        await _repository.UpdateAsync(evt);
        await _repository.SaveChangesAsync();

        return (await MapManyAsync(new List<Event> { evt })).Single();
    }

    private static List<Guid> MergeTagIds(List<EventTagRequestDto>? tags, List<Guid>? tagIds)
    {
        var merged = new List<Guid>();

        if (tags is not null)
            merged.AddRange(tags.Select(t => t.TagId));

        if (tagIds is not null)
            merged.AddRange(tagIds);

        return merged.Distinct().ToList();
    }

    private async Task AddValidatedTagsAsync(Event evt, List<Guid> tagIds)
    {
        if (tagIds.Count == 0)
            return;

        var validTags = await _tagGrpcClient.GetTagsAsync(tagIds);
        if (validTags is null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["tags"] = new[] { "Unable to validate tags. TagService is unavailable." }
            });

        var tagLookup = validTags
            .GroupBy(t => t.Id)
            .ToDictionary(g => g.Key, g => g.First().Name);

        var missing = tagIds.Where(id => !tagLookup.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["tags"] = new[] { $"Tag(s) not found: {string.Join(", ", missing)}" }
            });

        foreach (var tagId in tagIds)
            evt.AddTag(tagId, tagLookup[tagId]);
    }

    private static void ValidateQuery(EventListQueryDto query)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !Enum.TryParse<EventStatus>(query.Status, true, out _))
            errors["status"] = new[] { $"Invalid status '{query.Status}'. Valid values: Draft, Published, Cancelled, Completed" };

        if (!string.IsNullOrWhiteSpace(query.Visibility) &&
            !Enum.TryParse<EventVisibility>(query.Visibility, true, out _))
            errors["visibility"] = new[] { $"Invalid visibility '{query.Visibility}'. Valid values: Public, Private" };

        if (!string.IsNullOrWhiteSpace(query.Timeframe) &&
            query.Timeframe.ToLower() is not ("upcoming" or "past" or "all"))
            errors["timeframe"] = new[] { $"Invalid timeframe '{query.Timeframe}'. Valid values: upcoming, past, all" };

        if (!string.IsNullOrWhiteSpace(query.Sort) &&
            query.Sort.ToLower() is not ("date-asc" or "date-desc" or "created-desc" or "popularity"))
            errors["sort"] = new[] { $"Invalid sort '{query.Sort}'. Valid values: date-asc, date-desc, created-desc, popularity" };

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    private async Task<Dictionary<Guid, RsvpCounts>> GetCountsAsync(List<Guid> eventIds)
    {
        if (_rsvpGrpcClient is null || eventIds.Count == 0)
            return new Dictionary<Guid, RsvpCounts>();

        return await _rsvpGrpcClient.GetCountsAsync(eventIds);
    }

    private async Task<List<EventDto>> MapManyAsync(List<Event> events)
    {
        if (events.Count == 0)
            return new List<EventDto>();

        var eventIds = events.Select(e => e.Id).ToList();
        var counts = await GetCountsAsync(eventIds);

        var tagIds = events
            .SelectMany(e => e.EventTags.Select(t => t.TagId))
            .Distinct()
            .ToList();

        var colors = new Dictionary<Guid, string>();
        if (tagIds.Count > 0)
        {
            var tags = await _tagGrpcClient.GetTagsAsync(tagIds);
            if (tags is not null)
            {
                colors = tags
                    .GroupBy(t => t.Id)
                    .ToDictionary(g => g.Key, g => g.First().Color);
            }
        }

        return events.Select(e =>
        {
            counts.TryGetValue(e.Id, out var count);

            var tags = e.EventTags
                .Select(t => new EventTagDto(
                    t.TagId,
                    t.TagName,
                    colors.TryGetValue(t.TagId, out var color) ? color : FallbackTagColor))
                .ToList();

            return new EventDto(
                e.Id,
                e.Title,
                e.Description,
                e.Location,
                e.StartsAt,
                e.EndsAt,
                e.Capacity,
                e.OrganizerId,
                e.OrganizerName,
                e.Status.ToString(),
                e.Visibility.ToString(),
                count.Going,
                e.CreatedAt,
                tags,
                count.Maybe,
                e.UpdatedAt);
        }).ToList();
    }
}
