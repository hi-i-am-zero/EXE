using FluentValidation;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class GenerateTimelineOptionsCommandValidator : AbstractValidator<GenerateTimelineOptionsCommand>
{
    public GenerateTimelineOptionsCommandValidator()
    {
        RuleFor(x => x.Request.CampaignId).NotEmpty();
    }
}
