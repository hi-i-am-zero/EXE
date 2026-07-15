using AutoMapper;
using AutoWork.Application.Common.Exceptions;
using AutoWork.Application.DTOs.Posts;
using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Application.Interfaces.Services;
using AutoWork.Domain.Entities;
using MediatR;

namespace AutoWork.Application.Features.Schedules.Commands;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, PostScheduleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateScheduleCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PostScheduleDto> Handle(CreateScheduleCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedException("User is not authenticated.");
        }

        var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(command.PostId, cancellationToken);
        if (post is null || post.Project.UserId != userId)
        {
            throw new NotFoundException(nameof(Post), command.PostId);
        }

        if (command.ScheduledAt <= DateTime.UtcNow)
        {
            throw new BadRequestException("Scheduled time must be in the future.");
        }

        var existingSchedule = await _unitOfWork.Schedules.GetByPostIdAsync(command.PostId, cancellationToken);
        if (existingSchedule is not null)
        {
            existingSchedule.ScheduledAt = command.ScheduledAt;
            existingSchedule.Status = 1;
            existingSchedule.FailureReason = null;
            await _unitOfWork.Schedules.UpdateAsync(existingSchedule, cancellationToken);
            post.Status = 2;
            await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<PostScheduleDto>(existingSchedule);
        }

        var schedule = new PostSchedule
        {
            PostId = command.PostId,
            ScheduledAt = command.ScheduledAt,
            Status = 1,
            RetryCount = 0
        };

        await _unitOfWork.Schedules.AddAsync(schedule, cancellationToken);
        post.Status = 2;
        await _unitOfWork.Posts.UpdateAsync(post, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PostScheduleDto>(schedule);
    }
}
