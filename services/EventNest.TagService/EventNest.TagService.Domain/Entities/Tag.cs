using EventNest.Shared.Domain.Entities;

namespace EventNest.TagService.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = "#6366f1";

    private Tag() { }

    public static Tag Create(string name, string color)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
        return tag;
    }

    public void UpdateName(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateColor(string color)
    {
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
