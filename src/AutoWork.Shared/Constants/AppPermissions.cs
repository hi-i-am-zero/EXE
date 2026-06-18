namespace AutoWork.Shared.Constants;

public static class AppPermissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Plans
    {
        public const string View = "Plans.View";
        public const string Manage = "Plans.Manage";
    }

    public static class Subscriptions
    {
        public const string View = "Subscriptions.View";
        public const string Manage = "Subscriptions.Manage";
    }

    public static class Posts
    {
        public const string View = "Posts.View";
        public const string Create = "Posts.Create";
        public const string Update = "Posts.Update";
        public const string Delete = "Posts.Delete";
        public const string Publish = "Posts.Publish";
        public const string Schedule = "Posts.Schedule";
    }

    public static class Content
    {
        public const string Generate = "Content.Generate";
        public const string Approve = "Content.Approve";
    }

    public static class Credits
    {
        public const string View = "Credits.View";
        public const string Manage = "Credits.Manage";
    }

    public static class Payments
    {
        public const string View = "Payments.View";
        public const string Manage = "Payments.Manage";
    }

    public static class Affiliates
    {
        public const string View = "Affiliates.View";
        public const string Manage = "Affiliates.Manage";
    }

    public static class Channels
    {
        public const string Manage = "Channels.Manage";
    }

    public static class Media
    {
        public const string Upload = "Media.Upload";
        public const string Manage = "Media.Manage";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Manage = "Settings.Manage";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
    }

    public static class Notifications
    {
        public const string View = "Notifications.View";
    }

    public static class AuditLogs
    {
        public const string View = "AuditLogs.View";
    }

    public static class System
    {
        public const string Admin = "System.Admin";
    }

    public static readonly IReadOnlyList<string> All =
    [
        Users.View,
        Users.Create,
        Users.Update,
        Users.Delete,
        Plans.View,
        Plans.Manage,
        Subscriptions.View,
        Subscriptions.Manage,
        Posts.View,
        Posts.Create,
        Posts.Update,
        Posts.Delete,
        Posts.Publish,
        Posts.Schedule,
        Content.Generate,
        Content.Approve,
        Credits.View,
        Credits.Manage,
        Payments.View,
        Payments.Manage,
        Affiliates.View,
        Affiliates.Manage,
        Channels.Manage,
        Media.Upload,
        Media.Manage,
        Settings.View,
        Settings.Manage,
        Reports.View,
        Notifications.View,
        AuditLogs.View,
        System.Admin
    ];
}
