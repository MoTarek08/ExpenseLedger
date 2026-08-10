using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.ExpensesUseCases.CreateExpenseNamespace.Models;

namespace Infrastructure.Scheduling.BackgroundJobs
{
    public class GenerateExpenseFromScheduledExpense
    {
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly IBuildExpenseService _buildExpense;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateExpenseFromScheduledExpense(
            IBackgroundJobsService backgroundJobsService,
            IExpensesRepository expensesRepository,
            IScheduledExpensesRepository scheduledExpensesRepository,
            IBuildExpenseService buildExpense,
            IUnitOfWork unitOfWork
            )
        {
            _backgroundJobsService = backgroundJobsService;
            _expensesRepository = expensesRepository;
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _buildExpense = buildExpense;
            _unitOfWork = unitOfWork;
        }

        public async Task Execute(Guid scheduledExpenseId, DateOnly expectedDueDate)
        {
            var scheduledExpense = await _scheduledExpensesRepository.FindAsync(scheduledExpenseId);
            if (scheduledExpense is null || !scheduledExpense.IsActive)
                return;

            if (scheduledExpense.NextDueOn != expectedDueDate)
                return;

                var buildingExpenseResult = await _buildExpense.BuildExpense(
                scheduledExpense.UserId,
                new CreateExpenseRequestModel(
                scheduledExpense.CategoryId,
                scheduledExpense.Title,
                scheduledExpense.Amount,
                scheduledExpense.NextDueOn!.Value,
                scheduledExpense.SubCategoryId));

            if (buildingExpenseResult.IsFailure)
                return;

            var expense = buildingExpenseResult.Data!;
            expense.LinkToScheduledExpense(scheduledExpense.Id,scheduledExpense.NextDueOn.Value);
            _expensesRepository.Add(expense);
            scheduledExpense.MarkAsProcessed(scheduledExpense.NextDueOn.Value);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is GeneratedExpenseForThatDayAlreadyExists)
                    return;
                throw;
            }

            if (scheduledExpense.NextDueOn is not null)
            {
                _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                    scheduledExpenseId,
                    scheduledExpense.NextDueOn.Value);
            }

            _backgroundJobsService.TriggerAfterBackgroundExpenseCreationJobs(expense.Id);
        }
    }
}
