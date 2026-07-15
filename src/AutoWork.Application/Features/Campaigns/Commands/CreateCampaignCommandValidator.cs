using FluentValidation;

namespace AutoWork.Application.Features.Campaigns.Commands;

public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Tên chiến dịch là bắt buộc.").MaximumLength(200);
        RuleFor(x => x.Request.GoalId).NotEmpty().WithMessage("Mục tiêu chiến dịch là bắt buộc.");
        RuleFor(x => x.Request.PromotionTypeId).NotEmpty().WithMessage("Loại ưu đãi là bắt buộc.");
        RuleFor(x => x.Request.NumberOfPosts).GreaterThan(0).WithMessage("Số bài đăng phải lớn hơn 0.");
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
        RuleFor(x => x.Request.DiscountPercent)
            .InclusiveBetween(0, 100).When(x => x.Request.DiscountPercent.HasValue)
            .WithMessage("Phần trăm giảm giá phải từ 0-100.");
    }
}
