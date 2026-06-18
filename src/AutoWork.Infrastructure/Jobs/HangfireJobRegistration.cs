using Hangfire;

namespace AutoWork.Infrastructure.Jobs;

public static class HangfireJobRegistration
{
    public const string PostScheduleJobId = "post-schedule-processor";
    public static void RegisterRecurringJobs() =>
        RecurringJob.AddOrUpdate<PostScheduleJob>(PostScheduleJobId, job => job.ExecuteAsync(CancellationToken.None), Cron.Minutely);
}
