namespace Application.Interfaces.BackgroundJobs
{
public interface IBackgroundJobsService
{
    void AddOrUpdateCleanUpStaleExpenseFileObjectsWorker();
    // COMMENTED OUT: object storage deletion requests are no longer used
    //void AddOrUpdateObjectStorageDeletionCleanupWorker();
    void ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(Guid schduledExpenseId, DateOnly expectedDueDate);
    void TriggerAfterManualExpenseCreationJobs(Guid expenseId);
    void TriggerBackgroundCheckGoalAchivement(Guid expenseId);
    void TriggerAfterBackgroundExpenseCreationJobs(Guid expenseId);
}
}
