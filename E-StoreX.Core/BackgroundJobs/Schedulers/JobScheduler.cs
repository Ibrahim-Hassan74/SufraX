using EStoreX.Core.BackgroundJobs.Interfaces;
using Hangfire;

namespace EStoreX.Core.BackgroundJobs.Schedulers
{
    public static class JobScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate<IEmailJob>(
                "weekly-email-job",
                job => job.SendWeeklyEmailsAsync(null),
                //"45 15 * * 2"
                Cron.Never
            );
            RecurringJob.AddOrUpdate<IEmailJob>(
                "active-discount-email-job",
                job => job.SendActiveDiscountsEmailAsync(null),
                Cron.Never
            );
            RecurringJob.AddOrUpdate<IEmailJob>(
                "daily-sales-report-job",
                job => job.SendDailySalesReportAsync(null),
                //"0 7 * * *"
                Cron.Never
            );

            RecurringJob.AddOrUpdate<IEmailJob>(
                "mystery-email-job",
                job => job.SendMysteryLaunchEmailsAsync(null),
                //"45 15 * * 2"
                Cron.Never
            );
        }
    }
}
