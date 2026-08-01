using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.BusinessQueries;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.BudgetUseCases.Helpers;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;

namespace Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs
{
    public class CheckCategoryPreferenceViolation
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUserCategoryPreferencesRepository _categoryPreferenceRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IBudgetQueries _budgetQueries;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;

        public CheckCategoryPreferenceViolation(
            INotificationsRepository notificationsRepository,
            IUserCategoryPreferencesRepository categoryPreferenceRepository,
            IUsersRepository usersRepository,
            IExpensesRepository expensesRepository,
            IBudgetQueries budgetQueries,
            IUnitOfWork unitOfWork,
            IDateProvider dateTimeProvider)
        {
            _notificationsRepository = notificationsRepository;
            _categoryPreferenceRepository = categoryPreferenceRepository;
            _usersRepository = usersRepository;
            _expensesRepository = expensesRepository;
            _budgetQueries = budgetQueries;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task Execute(Guid expenseId)
        {
            var expense = await _expensesRepository.FindAsync(expenseId);
            if (expense is null)
                return;

            var preference = await _categoryPreferenceRepository.FindIncludingCategoryAsync(expense.UserId, expense.CategoryId);
            if (preference is null)
                return;

            var financialProfile = await _usersRepository.GetFinancialProfileByUserIdAsync(preference.UserId);
            if (financialProfile is null)
                return;

            var now = _dateTimeProvider.Now;
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            var lastPayDay = BudgetComputingHelpers.GetLastPayDay(financialProfile.ResetDay, today);

            if (expense.SpentOn < lastPayDay)
                return;

            Notification notification;

            if(preference.PreferenceLevel == CategoryPreferenceLevel.Avoid)
            {
                notification = Notification.SpendingOnAvoidPreference(
                    preference.UserId,
                    expense.Id,
                    preference.CategoryId,
                    preference.Category.Name,
                    lastPayDay,
                    now);
            }

            else if (preference.PreferenceLevel == CategoryPreferenceLevel.Minimal)
            {
                var amountSpent = await _budgetQueries.GetTotalSpentForCategoryAsync(
                    preference.UserId,
                    preference.CategoryId,
                    lastPayDay,
                    today);

                if (amountSpent > 0.05m * financialProfile.MonthlyNetIncome)
                {
                    notification = Notification.SpendingOnMinimalPreference(
                        preference.UserId,
                        expense.Id,
                        preference.CategoryId,
                        preference.Category.Name,
                        amountSpent,
                        lastPayDay,
                        now);
                }
                else { return; }
            }

            else { return; }

            if (await _notificationsRepository.ExistsByDedupKeyAsync(notification.UserId, notification.DedupKey))
                return;

            _notificationsRepository.Add(notification);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }

            catch (Exception ex) 
            {
                if (ex is NotificationDeuplicationKeyAlreadyExists)
                    return;
                throw;
            }
        }
    }
}
