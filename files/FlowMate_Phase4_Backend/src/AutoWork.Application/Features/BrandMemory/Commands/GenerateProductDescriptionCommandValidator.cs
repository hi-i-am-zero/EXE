using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class GenerateProductDescriptionCommandValidator : AbstractValidator<GenerateProductDescriptionCommand>
{
    public GenerateProductDescriptionCommandValidator()
    {
        RuleFor(x => x.Request.ProductId).NotEmpty();
    }
}
