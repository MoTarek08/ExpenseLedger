using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.BackgroundJobs;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.Models.Result;
using Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models;
using Domain.Entities.ScheduledExpenseNamespace;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense
{
    public class CreateScheduledExpenseUseCase
    {
        private readonly IScheduledExpensesRepository _scheduledExpensesRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobsService _backgroundJobsService;
        private readonly IDateProvider _dateProvider;
        private readonly ILogger<CreateScheduledExpenseUseCase> _logger;

        public CreateScheduledExpenseUseCase(
            IScheduledExpensesRepository scheduledExpensesRepository,
            ICategoriesRepository categoriesRepository,
            IUnitOfWork unitOfWork,
            IBackgroundJobsService backgroundJobsService,
            IDateProvider dateProvider,
            ILogger<CreateScheduledExpenseUseCase> logger
            )
        {
            _scheduledExpensesRepository = scheduledExpensesRepository;
            _categoriesRepository = categoriesRepository;
            _unitOfWork = unitOfWork;
            _backgroundJobsService = backgroundJobsService;
            _dateProvider = dateProvider;
            _logger = logger;
        }

        public async Task<Result<Guid>> Execute(
            Guid userId,
            CreateScheduledExpenseRequestModel requestModel,
            CancellationToken cancellationToken)
        {
            if (requestModel.SubCategoryId is not null)
            {
                if (!await _categoriesRepository.SubBelongsToMainAsync(requestModel.CategoryId, requestModel.SubCategoryId.Value, cancellationToken))
                    return Result<Guid>.Failure(new Error(ExpensesErrorCodes.EXPENSE_CATEGORIES_DO_NOT_BELONG_TO_EACH_OTHER));
            }

            var entry = ScheduledExpense.Create(
                userId,
                requestModel.Title,
                requestModel.Amount,
                requestModel.CategoryId,
                requestModel.SubCategoryId,
                requestModel.Cadence,
                requestModel.FirstDueOn,
                _dateProvider.Now);
            _scheduledExpensesRepository.Add(entry);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _backgroundJobsService.ScheduleGenerateExpenseFromScheduledExpenseOnNextDueDateWorker(
                entry.Id,
                entry.NextDueOn ?? entry.FirstDueOn);

            _logger.LogInformation("Scheduled expense created {ScheduledExpenseId} {UserId}", entry.Id, userId);
            return Result<Guid>.Success(entry.Id);
        }
    }
}
