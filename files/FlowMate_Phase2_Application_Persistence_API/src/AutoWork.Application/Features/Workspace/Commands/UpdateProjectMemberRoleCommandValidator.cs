using FluentValidation;

namespace AutoWork.Application.Features.Workspace.Commands;

public class UpdateProjectMemberRoleCommandValidator : AbstractValidator<UpdateProjectMemberRoleCommand>
{
    private static readonly string[] AllowedRoles = ["Owner", "Admin", "Member"];

    public UpdateProjectMemberRoleCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.UserId).NotEmpty();
        RuleFor(x => x.Request.Role).Must(r => AllowedRoles.Contains(r))
            .WithMessage("Vai trò phải là Owner, Admin hoặc Member.");
    }
}
