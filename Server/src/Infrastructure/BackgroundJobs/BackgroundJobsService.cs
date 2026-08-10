using Application.Interfaces.BackgroundJobs;
using Hangfire;
using Infrastructure.BackgroundJobs.BackgroundJobs;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;
using Infrastructure.Scheduling.BackgroundJobs;

namespace Infrastructure.BackgroundJobs
{
    public class BackgroundJobsService : IBackgroundJobsService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;

        public BackgroundJobsService(
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobManager)
        {
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
        }

        public void AddOrUpdateCleanUpStaleExpenseFileObjectsWorker()
        {
            _recurringJobManager.AddOrUpdate<CleanupStaleExpensesFileObjectsRecords>(
                "clenup-stale-expenses-file-objects",
                job => job.Execute(),
                Cron.Hourly());
        }

        // COMMENTED OUT: object storage deletion requests are no longer used
        //public void AddOrUpdateObjectStorageDeletionCleanupWorker()
        //{
        //    _recurringJobManager.AddOrUpdate<ObjectStorageDeletionCleanupJob>(
        //        "execute-object-storage-deletion-requests",
        //        job => job.Execute(),
        //        Cron.Daily());
        //}

        public void ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(Guid schduledExpenseId, DateOnly expectedDueDate)
        {
            _backgroundJobClient.Schedule<GenerateExpenseFromScheduledExpense>(
            job => job.Execute(schduledExpenseId, expectedDueDate),
            new DateTimeOffset(
                expectedDueDate,
                TimeOnly.MinValue,
                TimeSpan.Zero));
        }

        public void TriggerAfterManualExpenseCreationJobs(Guid expenseId)
        {
            _backgroundJobClient.Enqueue<CheckCategoryPreferenceViolation>(job => job.Execute(expenseId));
            _backgroundJobClient.Enqueue<CheckGoalAchievement>(job => job.Execute(expenseId));
        }

        public void TriggerBackgroundCheckGoalAchivement(Guid expenseId)
        {
            _backgroundJobClient.Enqueue<CheckGoalAchievement>(job => job.Execute(expenseId));
        }

        public void TriggerAfterBackgroundExpenseCreationJobs(Guid expenseId)
        {
            _backgroundJobClient.Enqueue<CheckBudgetAfterExpenseCreationJob>(job => job.Execute(expenseId));
            _backgroundJobClient.Enqueue<CheckCategoryPreferenceViolation>(job => job.Execute(expenseId));
            _backgroundJobClient.Enqueue<CheckGoalAchievement>(job => job.Execute(expenseId));
            _backgroundJobClient.Enqueue<CreateScheduledExpenseGeneratedNotification>(job => job.Execute(expenseId));
        }
    }
}
