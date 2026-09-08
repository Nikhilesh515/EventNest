namespace EventNest.AuthService.Application.Authorization;

public static class EventNestPermissions
{
    public const string GroupEvents = "Events";
    public const string GroupTags = "Tags";
    public const string GroupRsvps = "RSVPs";
    public const string GroupUsers = "Users";

    // Events
    public const string EventsView = "Events.View";
    public const string EventsCreate = "Events.Create";
    public const string EventsEdit = "Events.Edit";
    public const string EventsDelete = "Events.Delete";

    // Tags
    public const string TagsView = "Tags.View";
    public const string TagsCreate = "Tags.Create";
    public const string TagsEdit = "Tags.Edit";
    public const string TagsDelete = "Tags.Delete";

    // RSVPs
    public const string RsvpsView = "RSVPs.View";
    public const string RsvpsCreate = "RSVPs.Create";
    public const string RsvpsManage = "RSVPs.Manage";
    public const string RsvpsCancel = "RSVPs.Cancel";

    // Users
    public const string UsersView = "Users.View";
    public const string UsersManage = "Users.Manage";
}
