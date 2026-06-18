namespace AutoWork.Shared.Constants;

public static class AppRoles
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string Admin = nameof(Admin);
    public const string User = nameof(User);

    public static readonly IReadOnlyList<string> All =
    [
        SuperAdmin,
        Admin,
        User
    ];
}
