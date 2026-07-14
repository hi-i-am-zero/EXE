using AutoWork.Application.Features.AI.Commands;
using FluentValidation;

namespace AutoWork.Application.Features.AI.Commands;

public class GenerateContentCommandValidator : AbstractValidator<GenerateContentCommand>
{
    public GenerateContentCommandValidator()
    {
        RuleFor(x => x.Request.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(500);

        RuleFor(x => x.Request.Length)
            .InclusiveBetween(100, 5000).WithMessage("Length must be between 100 and 5000.");

        RuleFor(x => x.Request.Provider)
            .IsInEnum().WithMessage("Invalid AI provider.");
    }
}
