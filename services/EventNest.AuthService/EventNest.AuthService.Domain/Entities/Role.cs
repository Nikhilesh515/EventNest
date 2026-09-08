namespace EventNest.AuthService.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    private Role() { }

    public static Role Create(string name, string displayName, string? description = null, int sortOrder = 0)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            DisplayName = displayName,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }
}
