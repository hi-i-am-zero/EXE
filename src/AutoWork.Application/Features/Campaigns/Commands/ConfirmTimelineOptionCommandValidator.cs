using FluentValidation;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class ConfirmTimelineOptionCommandValidator : AbstractValidator<ConfirmTimelineOptionCommand>
{
    public ConfirmTimelineOptionCommandValidator()
    {
        RuleFor(x => x.Request.CampaignId).NotEmpty();
        RuleFor(x => x.Request.TemplateTypeId).NotEmpty();
        RuleFor(x => x.Request.TimelineName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Posts).Must(p => p.Count > 0).WithMessage("Phương án phải có ít nhất 1 bài viết.");
    }
}
