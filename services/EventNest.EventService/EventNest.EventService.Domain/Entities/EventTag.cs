namespace EventNest.EventService.Domain.Entities;

public class EventTag
{
    public Guid EventId { get; private set; }
    public Guid TagId { get; private set; }
    public string TagName { get; private set; } = string.Empty;

    private EventTag() { }

    public static EventTag Create(Guid eventId, Guid tagId, string tagName)
    {
        return new EventTag
        {
            EventId = eventId,
            TagId = tagId,
            TagName = tagName
        };
    }
}
