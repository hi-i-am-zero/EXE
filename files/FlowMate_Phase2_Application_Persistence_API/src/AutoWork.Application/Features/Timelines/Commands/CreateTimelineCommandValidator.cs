using FluentValidation;

namespace AutoWork.Application.Features.Timelines.Commands;

public class CreateTimelineCommandValidator : AbstractValidator<CreateTimelineCommand>
{
    public CreateTimelineCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Tên timeline là bắt buộc.").MaximumLength(200);
        RuleFor(x => x.Request.TemplateTypeId).NotEmpty().WithMessage("Vui lòng chọn mẫu timeline.");
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .WithMessage("Ngày kết thúc phải sau ngày bắt đầu.");
    }
}
