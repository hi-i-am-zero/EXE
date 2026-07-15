using FluentValidation;

namespace AutoWork.Application.Features.Workspace.Commands;

public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    private static readonly string[] AllowedRoles = ["Owner", "Admin", "Member"];

    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty().WithMessage("Workspace là bắt buộc.");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Request.Role)
            .Must(r => AllowedRoles.Contains(r))
            .WithMessage("Vai trò phải là Owner, Admin hoặc Member.");
    }
}
