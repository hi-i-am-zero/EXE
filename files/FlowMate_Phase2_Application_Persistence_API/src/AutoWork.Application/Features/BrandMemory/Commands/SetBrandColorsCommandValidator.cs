using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandColorsCommandValidator : AbstractValidator<SetBrandColorsCommand>
{
    private static readonly string HexPattern = "^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$";

    public SetBrandColorsCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleForEach(x => x.Request.Colors).ChildRules(color =>
        {
            color.RuleFor(c => c.ColorHex).Matches(HexPattern).WithMessage("Mã màu phải dạng #RRGGBB.");
            color.RuleFor(c => c.ColorType).Must(t => t is "Primary" or "Secondary")
                .WithMessage("Loại màu phải là Primary hoặc Secondary.");
        });
    }
}
