using FluentValidation;

namespace AutoWork.Application.Features.BrandMemory.Commands;

public class SetBrandKeywordsCommandValidator : AbstractValidator<SetBrandKeywordsCommand>
{
    public SetBrandKeywordsCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.SelectedIds)
            .Must(ids => ids.Count <= 5)
            .WithMessage("Chỉ được chọn tối đa 5 từ khóa mô tả giọng văn.");
        // Không ép NotEmpty/tối thiểu 3 ở đây: cho phép người dùng lưu tạm khi đang điền dở form.
    }
}
