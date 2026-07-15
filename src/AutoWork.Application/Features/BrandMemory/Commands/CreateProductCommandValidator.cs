using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Price).GreaterThanOrEqualTo(0).When(x => x.Request.Price.HasValue);
        RuleFor(x => x.Request.MediaFileIds).Must(ids => ids.Count <= 4)
            .WithMessage("Chỉ được đính tối đa 4 ảnh sản phẩm."); // khớp UI "Sản phẩm của bạn (tối đa 4 ảnh)"
    }
}
