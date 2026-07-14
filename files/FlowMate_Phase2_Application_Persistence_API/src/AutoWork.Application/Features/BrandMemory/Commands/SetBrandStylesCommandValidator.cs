using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandStylesCommandValidator : AbstractValidator<SetBrandStylesCommand>
{
    public SetBrandStylesCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.SelectedIds)
            .Must(ids => ids.Count <= 2)
            .WithMessage("Chỉ được chọn tối đa 2 phong cách thương hiệu.");
    }
}
