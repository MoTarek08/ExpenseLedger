using Application.Interfaces.BackgroundJobs;

namespace IntegrationTests.BackgroundJobs
{
    public class FakeBackgroundJobsService : IBackgroundJobsService
    {

        public List<(Guid ScheduledExpenseId, DateOnly ExpectedDueDate)> ScheduledExpenseGenerationJobs { get; } = new();
        public List<Guid> GeneratedExpensesIdsThatTriggeredBackgroundJobs = new();
        public List<Guid> ManualExpensesIdsThatTriggeredBackgroundJobs = new();
        public List<Guid> ExpensesIdsThatTriggeredCheckGoalAchievement= new();

        public void AddOrUpdateCleanUpStaleExpenseFileObjectsWorker() {}

        public void AddOrUpdateObjectStorageDeletionCleanupWorker() {}

        public void ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(Guid schduledExpenseId, DateOnly expectedDueDate)
        {
            ScheduledExpenseGenerationJobs.Add((schduledExpenseId, expectedDueDate));
        }

        public void TriggerAfterBackgroundExpenseCreationJobs(Guid expenseId)
        {
            GeneratedExpensesIdsThatTriggeredBackgroundJobs.Add(expenseId);
        }

        public void TriggerAfterManualExpenseCreationJobs(Guid expenseId)
        {
            ManualExpensesIdsThatTriggeredBackgroundJobs.Add(expenseId);
        }

        public void TriggerBackgroundCheckGoalAchivement(Guid expenseId)
        {
            ExpensesIdsThatTriggeredCheckGoalAchievement.Add(expenseId);
        }
    }
}
