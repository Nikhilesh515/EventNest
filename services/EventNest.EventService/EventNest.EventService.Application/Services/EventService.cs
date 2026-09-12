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

    public EventService(IEventRepository repository, ITagGrpcClient tagGrpcClient)
    {
        _repository = repository;
        _tagGrpcClient = tagGrpcClient;
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
        return evt is null ? null : MapToDto(evt);
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return events.Select(MapToDto).ToList();
    }

    public async Task<List<EventDto>> GetByOrganizerAsync(Guid organizerId)
    {
        var events = await _repository.GetByOrganizerAsync(organizerId);
        return events.Select(MapToDto).ToList();
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
            evt.CreatedAt,
            evt.EventTags.Select(t => new EventTagDto(t.TagId, t.TagName)).ToList());
    }
}
