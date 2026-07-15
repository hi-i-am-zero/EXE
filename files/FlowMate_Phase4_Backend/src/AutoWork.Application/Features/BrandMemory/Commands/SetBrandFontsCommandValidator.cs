using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandFontsCommandValidator : AbstractValidator<SetBrandFontsCommand>
{
    public SetBrandFontsCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleForEach(x => x.Request.Fonts).ChildRules(font =>
        {
            font.RuleFor(f => f.FontName).NotEmpty().MaximumLength(100);
            font.RuleFor(f => f.UsageType).Must(t => t is "Title" or "Body")
                .WithMessage("Loại font phải là Title hoặc Body.");
        });
    }
}
