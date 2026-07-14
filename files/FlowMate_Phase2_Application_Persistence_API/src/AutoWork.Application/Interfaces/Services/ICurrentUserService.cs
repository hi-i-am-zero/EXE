namespace AutoWork.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    IReadOnlyList<string> Permissions { get; }

    bool IsInRole(string role);
}
