using AutoWork.Application.Features.Posts.Commands;
using FluentValidation;

namespace AutoWork.Application.Features.Posts.Commands;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Request.ProjectId)
            .NotEmpty().WithMessage("Project is required.");

        RuleFor(x => x.Request.ChannelAccountId)
            .NotEmpty().WithMessage("Channel account is required.");

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500);

        RuleFor(x => x.Request.Content)
            .NotEmpty().WithMessage("Content is required.");
    }
}
