using FluentValidation;

namespace AutoWork.Application.Features.Workspace.Commands;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên workspace là bắt buộc.")
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .MaximumLength(2000);
    }
}
