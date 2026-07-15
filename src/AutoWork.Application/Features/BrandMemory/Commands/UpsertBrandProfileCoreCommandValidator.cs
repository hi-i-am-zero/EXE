using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class UpsertBrandProfileCoreCommandValidator : AbstractValidator<UpsertBrandProfileCoreCommand>
{
    public UpsertBrandProfileCoreCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();

        RuleFor(x => x.Request.BusinessName)
            .NotEmpty().WithMessage("Tên doanh nghiệp là bắt buộc.")
            .MaximumLength(150);

        RuleFor(x => x.Request.BrandName)
            .NotEmpty().WithMessage("Tên thương hiệu là bắt buộc.")
            .MaximumLength(150);

        RuleFor(x => x.Request.Industry).MaximumLength(100);
        RuleFor(x => x.Request.ShortDescription).MaximumLength(1000);
        RuleFor(x => x.Request.Address).MaximumLength(300);
        RuleFor(x => x.Request.ContactPhone).MaximumLength(20);
        RuleFor(x => x.Request.Website).MaximumLength(255);

        RuleFor(x => x.Request.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Request.ContactEmail))
            .WithMessage("Email liên hệ không hợp lệ.");

        RuleFor(x => x.Request.FoundingYear)
            .InclusiveBetween((short)1900, (short)DateTime.UtcNow.Year)
            .When(x => x.Request.FoundingYear.HasValue)
            .WithMessage("Năm thành lập không hợp lệ.");
    }
}
