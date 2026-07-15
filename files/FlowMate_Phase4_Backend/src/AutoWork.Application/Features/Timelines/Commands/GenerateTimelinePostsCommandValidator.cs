using FluentValidation;

namespace AutoWork.Application.Features.Timelines.Commands;

public class GenerateTimelinePostsCommandValidator : AbstractValidator<GenerateTimelinePostsCommand>
{
    public GenerateTimelinePostsCommandValidator()
    {
        RuleFor(x => x.Request.TimelineId).NotEmpty();
        RuleFor(x => x.Request.PostCount)
            .InclusiveBetween(1, 30).When(x => x.Request.PostCount.HasValue)
            .WithMessage("Số bài viết phải từ 1-30.");
    }
}
