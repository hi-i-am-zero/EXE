using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.CategoryName).NotEmpty().MaximumLength(100);
    }
}
