using FluentValidation;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class GenerateCampaignProductDescriptionCommandValidator : AbstractValidator<GenerateCampaignProductDescriptionCommand>
{
    public GenerateCampaignProductDescriptionCommandValidator()
    {
        RuleFor(x => x.Request.CampaignId).NotEmpty();
        RuleFor(x => x.Request.MediaFileIds).Must(ids => ids.Count > 0 && ids.Count <= 4)
            .WithMessage("Vui lòng upload từ 1 đến 4 ảnh sản phẩm.");
    }
}
