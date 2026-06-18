using Microsoft.AspNetCore.Authorization;

namespace AutoWork.API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (string.IsNullOrWhiteSpace(requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissions = context.User.FindAll("permission").Select(c => c.Value);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
