using AutoWork.Application.Features.Posts.Commands;
using FluentValidation;

namespace AutoWork.Application.Features.Posts.Commands;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId)
            .NotEmpty().WithMessage("Project is required.");

        // FlowMate v2: chấp nhận ChannelAccountId đơn lẻ (legacy) hoặc ChannelAccountIds đa chọn —
        // miễn tổng hợp lại phải có ít nhất 1 nền tảng được chọn.
        RuleFor(x => x)
            .Must(x => x.Request.ChannelAccountId != Guid.Empty || x.Request.ChannelAccountIds.Count > 0)
            .WithMessage("Vui lòng chọn ít nhất 1 nền tảng để đăng bài.");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500);

        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Request.Hashtags)
            .Must(h => h.Count <= 30).WithMessage("Chỉ được thêm tối đa 30 hashtag.");
    }
}
