namespace Application.Interfaces.BackgroundJobs
{
public interface IBackgroundJobsService
{
    void AddOrUpdateCleanUpStaleExpenseFileObjectsWorker();
    void AddOrUpdateObjectStorageDeletionCleanupWorker();
    void ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(Guid schduledExpenseId, DateOnly expectedDueDate);
    void TriggerAfterManualExpenseCreationJobs(Guid expenseId);
    void TriggerBackgroundCheckGoalAchivement(Guid expenseId);
    void TriggerAfterBackgroundExpenseCreationJobs(Guid expenseId);
}
}
