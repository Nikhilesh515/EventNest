namespace EventNest.Shared.Application.Authorization;

public static class EventNestPermissions
{
    public const string GroupEvents = "Events";
    public const string GroupTags = "Tags";
    public const string GroupRSVPs = "RSVPs";
    public const string GroupUsers = "Users";

    public static class Events
    {
        public const string View = GroupEvents + ".View";
        public const string Create = GroupEvents + ".Create";
        public const string Edit = GroupEvents + ".Edit";
        public const string Delete = GroupEvents + ".Delete";
    }

    public static class Tags
    {
        public const string View = GroupTags + ".View";
        public const string Create = GroupTags + ".Create";
        public const string Edit = GroupTags + ".Edit";
        public const string Delete = GroupTags + ".Delete";
    }

    public static class RSVPs
    {
        public const string View = GroupRSVPs + ".View";
        public const string Create = GroupRSVPs + ".Create";
        public const string Edit = GroupRSVPs + ".Edit";
        public const string Manage = GroupRSVPs + ".Manage";
        public const string Cancel = GroupRSVPs + ".Cancel";
    }

    public static class Users
    {
        public const string View = GroupUsers + ".View";
        public const string Manage = GroupUsers + ".Manage";
    }

    public static readonly Dictionary<string, List<string>> All = new()
    {
        [GroupEvents] = new() { Events.View, Events.Create, Events.Edit, Events.Delete },
        [GroupTags] = new() { Tags.View, Tags.Create, Tags.Edit, Tags.Delete },
        [GroupRSVPs] = new() { RSVPs.View, RSVPs.Create, RSVPs.Edit, RSVPs.Manage, RSVPs.Cancel },
        [GroupUsers] = new() { Users.View, Users.Manage }
    };

    public static readonly Dictionary<string, List<string>> RoleDefaults = new()
    {
        ["User"] = new() { Events.View, Tags.View, RSVPs.View, RSVPs.Create, RSVPs.Edit, RSVPs.Cancel },
        ["Organizer"] = new() { Events.View, Events.Create, Events.Edit, Tags.View, Tags.Create, RSVPs.View, RSVPs.Manage },
        ["Moderator"] = new() { Events.View, Events.Create, Events.Edit, Tags.View, Tags.Create, RSVPs.View, RSVPs.Manage, Users.View },
        ["Admin"] = new(),
        ["SuperAdmin"] = new()
    };

    public static readonly IReadOnlyList<string> AllNames =
        All.SelectMany(g => g.Value).Distinct().ToList();

    public static readonly HashSet<string> BuiltInRoleNames = new(StringComparer.Ordinal)
    {
        "User", "Organizer", "Moderator", "Admin", "SuperAdmin"
    };

    public static bool IsValid(string permissionName) =>
        AllNames.Contains(permissionName);

    public static bool IsBuiltInRole(string roleName) =>
        BuiltInRoleNames.Contains(roleName);
}
