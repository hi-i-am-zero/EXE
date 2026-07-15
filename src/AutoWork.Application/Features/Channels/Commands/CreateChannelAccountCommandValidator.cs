using FluentValidation;

namespace AutoWork.Application.Features.Channels.Commands;

public class CreateChannelAccountCommandValidator : AbstractValidator<CreateChannelAccountCommand>
{
    public CreateChannelAccountCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId).NotEmpty();
        RuleFor(x => x.Request.ChannelId).NotEmpty().WithMessage("Vui lòng chọn nền tảng.");
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Tên hiển thị là bắt buộc.").MaximumLength(200);
    }
}
