using AutoWork.Application.Interfaces.Repositories;
using AutoWork.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace AutoWork.Infrastructure.Jobs;

public class PostScheduleJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<PostScheduleJob> _logger;

    public PostScheduleJob(IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient, ILogger<PostScheduleJob> logger)
    {
        _unitOfWork = unitOfWork; _backgroundJobClient = backgroundJobClient; _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        foreach (var schedule in await _unitOfWork.Schedules.GetPendingSchedulesAsync(DateTime.UtcNow, cancellationToken))
        {
            if (schedule.Status != (int)PostScheduleStatus.Pending) continue;
            schedule.Status = (int)PostScheduleStatus.Processing; schedule.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Schedules.UpdateAsync(schedule, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _backgroundJobClient.Enqueue<PublishPostJob>(j => j.ExecuteAsync(schedule.PostId, CancellationToken.None));
            _logger.LogInformation("Enqueued publish for post {PostId}", schedule.PostId);
        }
    }
}
