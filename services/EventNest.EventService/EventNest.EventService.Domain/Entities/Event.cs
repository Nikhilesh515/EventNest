using EventNest.EventService.Domain.Enums;
using EventNest.Shared.Domain.Entities;

namespace EventNest.EventService.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Location { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public int Capacity { get; private set; }
    public Guid OrganizerId { get; private set; }
    public string OrganizerName { get; private set; } = string.Empty;
    public EventStatus Status { get; private set; }
    public EventVisibility Visibility { get; private set; }
    public ICollection<EventTag> EventTags { get; private set; } = new List<EventTag>();

    private Event() { }

    public static Event Create(
        string title,
        string? description,
        string? location,
        DateTime startsAt,
        DateTime endsAt,
        int capacity,
        Guid organizerId,
        string organizerName)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Location = location,
            StartsAt = ToUtc(startsAt),
            EndsAt = ToUtc(endsAt),
            Capacity = capacity,
            OrganizerId = organizerId,
            OrganizerName = organizerName,
            Status = EventStatus.Draft,
            Visibility = EventVisibility.Public,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(string? location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(DateTime startsAt, DateTime endsAt)
    {
        StartsAt = ToUtc(startsAt);
        EndsAt = ToUtc(endsAt);
        UpdatedAt = DateTime.UtcNow;
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public void UpdateCapacity(int capacity)
    {
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(Guid tagId, string tagName)
    {
        if (!EventTags.Any(t => t.TagId == tagId))
        {
            EventTags.Add(EventTag.Create(Id, tagId, tagName));
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = EventTags.FirstOrDefault(t => t.TagId == tagId);
        if (tag is not null)
        {
            EventTags.Remove(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateVisibility(EventVisibility visibility)
    {
        Visibility = visibility;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = EventStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}
