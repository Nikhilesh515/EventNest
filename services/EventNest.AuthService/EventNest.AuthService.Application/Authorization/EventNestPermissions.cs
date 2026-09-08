namespace EventNest.AuthService.Application.Authorization;

public static class EventNestPermissions
{
    // Groups
    public const string GroupEvents = "Events";
    public const string GroupTags = "Tags";
    public const string GroupRsvps = "RSVPs";
    public const string GroupUsers = "Users";

    // Events
    public const string EventsView = GroupEvents + ".View";
    public const string EventsCreate = GroupEvents + ".Create";
    public const string EventsEdit = GroupEvents + ".Edit";
    public const string EventsDelete = GroupEvents + ".Delete";

    // Tags
    public const string TagsView = GroupTags + ".View";
    public const string TagsCreate = GroupTags + ".Create";
    public const string TagsEdit = GroupTags + ".Edit";
    public const string TagsDelete = GroupTags + ".Delete";

    // RSVPs
    public const string RsvpsView = GroupRsvps + ".View";
    public const string RsvpsCreate = GroupRsvps + ".Create";
    public const string RsvpsManage = GroupRsvps + ".Manage";
    public const string RsvpsCancel = GroupRsvps + ".Cancel";

    // Users
    public const string UsersView = GroupUsers + ".View";
    public const string UsersManage = GroupUsers + ".Manage";
}
