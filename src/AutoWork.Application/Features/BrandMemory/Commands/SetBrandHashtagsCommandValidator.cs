using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandHashtagsCommandValidator : AbstractValidator<SetBrandHashtagsCommand>
{
    public SetBrandHashtagsCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.SelectedIds)
            .Must(ids => ids.Count <= 30)
            .WithMessage("Chỉ được chọn tối đa 30 hashtag."); // khớp giới hạn "0/30" hiển thị trên UI
    }
}
