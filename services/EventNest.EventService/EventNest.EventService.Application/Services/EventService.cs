using EventNest.EventService.Application.DTOs.Events;
using EventNest.EventService.Application.Services.Interfaces;
using EventNest.EventService.Domain.Entities;
using EventNest.EventService.Domain.Enums;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.EventService.Application.Services;

public class EventService : IEventService
{
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

        await AddValidatedTagsAsync(evt, request.Tags);

        await _repository.AddAsync(evt);
        await _repository.SaveChangesAsync();

        return MapToDto(evt);
    }

    public async Task<EventDto?> GetByIdAsync(Guid id)
    {
        var evt = await _repository.GetByIdAsync(id);
        if (evt is null)
            return null;

        var counts = await GetGoingCountsAsync(new List<Guid> { id });
        counts.TryGetValue(id, out var going);
        return MapToDto(evt) with { Going = going };
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return events.Select(MapToDto).ToList();
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
            var counts = await GetGoingCountsAsync(events.Select(e => e.Id).ToList());
            events = events
                .OrderByDescending(e => counts.TryGetValue(e.Id, out var c) ? c : 0)
                .ToList();

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 1);
            events = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        var countsByEvent = await GetGoingCountsAsync(events.Select(e => e.Id).ToList());
        var items = events.Select(e =>
        {
            countsByEvent.TryGetValue(e.Id, out var going);
            return MapToDto(e) with { Going = going };
        }).ToList();

        return new PagedResultDto<EventDto>(items, total, query.Page, query.PageSize, pages);
    }

    public async Task<List<EventDto>> GetByOrganizerAsync(Guid organizerId)
    {
        var events = await _repository.GetByOrganizerAsync(organizerId);
        var counts = await GetGoingCountsAsync(events.Select(e => e.Id).ToList());
        return events.Select(e =>
        {
            counts.TryGetValue(e.Id, out var going);
            return MapToDto(e) with { Going = going };
        }).ToList();
    }

    public async Task<List<EventDto>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<EventStatus>(status, true, out var eventStatus))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["status"] = new[] { $"Invalid status '{status}'. Valid values: Draft, Published, Cancelled, Completed" }
            });

        var events = await _repository.GetByStatusAsync(eventStatus);
        return events.Select(MapToDto).ToList();
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
        await AddValidatedTagsAsync(evt, request.Tags);

        await _repository.UpdateAsync(evt);
        await _repository.SaveChangesAsync();

        return MapToDto(evt);
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

        return MapToDto(evt);
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

        return MapToDto(evt);
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

        return MapToDto(evt);
    }

    private async Task AddValidatedTagsAsync(Event evt, List<EventTagRequestDto> tags)
    {
        if (tags.Count == 0)
            return;

        var tagIds = tags.Select(t => t.TagId).Distinct().ToList();
        var validTags = await _tagGrpcClient.GetTagsAsync(tagIds);
        if (validTags is null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["tags"] = new[] { "Unable to validate tags. TagService is unavailable." }
            });

        var tagLookup = validTags.GroupBy(t => t.Id).ToDictionary(g => g.Key, g => g.First().Name);
        var missing = tagIds.Where(id => !tagLookup.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["tags"] = new[] { $"Tag(s) not found: {string.Join(", ", missing)}" }
            });

        foreach (var tag in tags)
            evt.AddTag(tag.TagId, tagLookup[tag.TagId]);
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

    private async Task<Dictionary<Guid, int>> GetGoingCountsAsync(List<Guid> eventIds)
    {
        if (_rsvpGrpcClient is null || eventIds.Count == 0)
            return new Dictionary<Guid, int>();

        return await _rsvpGrpcClient.GetGoingCountsAsync(eventIds);
    }

    private static EventDto MapToDto(Event evt)
    {
        return new EventDto(
            evt.Id,
            evt.Title,
            evt.Description,
            evt.Location,
            evt.StartsAt,
            evt.EndsAt,
            evt.Capacity,
            evt.OrganizerId,
            evt.OrganizerName,
            evt.Status.ToString(),
            evt.Visibility.ToString(),
            0,
            evt.CreatedAt,
            evt.EventTags.Select(t => new EventTagDto(t.TagId, t.TagName)).ToList());
    }
}
