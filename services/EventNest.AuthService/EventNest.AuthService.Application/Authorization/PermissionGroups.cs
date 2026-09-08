namespace EventNest.AuthService.Application.Authorization;

public static class PermissionGroups
{
    public static readonly Dictionary<string, List<string>> All = new()
    {
        [EventNestPermissions.GroupEvents] = new()
        {
            EventNestPermissions.EventsView,
            EventNestPermissions.EventsCreate,
            EventNestPermissions.EventsEdit,
            EventNestPermissions.EventsDelete
        },
        [EventNestPermissions.GroupTags] = new()
        {
            EventNestPermissions.TagsView,
            EventNestPermissions.TagsCreate,
            EventNestPermissions.TagsEdit,
            EventNestPermissions.TagsDelete
        },
        [EventNestPermissions.GroupRsvps] = new()
        {
            EventNestPermissions.RsvpsView,
            EventNestPermissions.RsvpsCreate,
            EventNestPermissions.RsvpsManage,
            EventNestPermissions.RsvpsCancel
        },
        [EventNestPermissions.GroupUsers] = new()
        {
            EventNestPermissions.UsersView,
            EventNestPermissions.UsersManage
        }
    };

    public static readonly Dictionary<string, List<string>> RoleDefaults = new()
    {
        ["User"] = new()
        {
            EventNestPermissions.EventsView,
            EventNestPermissions.TagsView,
            EventNestPermissions.RsvpsView,
            EventNestPermissions.RsvpsCreate
        },
        ["Organizer"] = new()
        {
            EventNestPermissions.EventsView,
            EventNestPermissions.EventsCreate,
            EventNestPermissions.EventsEdit,
            EventNestPermissions.TagsView,
            EventNestPermissions.TagsCreate,
            EventNestPermissions.RsvpsView,
            EventNestPermissions.RsvpsManage
        },
        ["Moderator"] = new()
        {
            EventNestPermissions.EventsView,
            EventNestPermissions.EventsCreate,
            EventNestPermissions.EventsEdit,
            EventNestPermissions.TagsView,
            EventNestPermissions.TagsCreate,
            EventNestPermissions.RsvpsView,
            EventNestPermissions.RsvpsManage,
            EventNestPermissions.UsersView
        },
        ["Admin"] = new(),
        ["SuperAdmin"] = new()
    };
}
