using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandCtasCommandValidator : AbstractValidator<SetBrandCtasCommand>
{
    public SetBrandCtasCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
    }
}
