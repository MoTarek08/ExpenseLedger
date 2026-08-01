using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.BusinessQueries;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.DomainEnums;
using Domain.Entities.ExpenseCategoryNamespace;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.Notification;
using Domain.Entities.UserCategoryPreferenceNamespace;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class CheckCategoryPreferenceViolationTests
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUserCategoryPreferencesRepository _categoryPreferenceRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IBudgetQueries _budgetQueries;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly CheckCategoryPreferenceViolation _sut;

        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid ExpenseId = Guid.NewGuid();
        private static readonly Guid CategoryId = Guid.NewGuid();
        private readonly DateTimeOffset _now = new DateTimeOffset(2026, 8, 15, 10, 0, 0, TimeSpan.Zero);

        public CheckCategoryPreferenceViolationTests()
        {
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _categoryPreferenceRepository = A.Fake<IUserCategoryPreferencesRepository>();
            _usersRepository = A.Fake<IUsersRepository>();
            _expensesRepository = A.Fake<IExpensesRepository>();
            _budgetQueries = A.Fake<IBudgetQueries>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();

            A.CallTo(() => _dateTimeProvider.Now).Returns(_now);

            _sut = new CheckCategoryPreferenceViolation(
                _notificationsRepository, _categoryPreferenceRepository, _usersRepository,
                _expensesRepository, _budgetQueries, _unitOfWork, _dateTimeProvider);
        }

        private Expense CreateExpense(DateOnly spentOn) =>
            Expense.CreateManualExpense(UserId, CategoryId, "Test", 100, spentOn, _now.AddHours(-1));

        private UserFinancialProfile CreateProfile(int resetDay = 1, decimal monthlyIncome = 10000) =>
            UserFinancialProfile.Create(UserId, monthlyIncome, resetDay, _now.AddDays(-30));

        private ExpenseCategory CreateCategory() =>
            ExpenseCategory.Create("FOOD", "Food", "Food expenses");

        private UserCategoryPreference CreatePreference(CategoryPreferenceLevel level, ExpenseCategory category)
        {
            var preference = UserCategoryPreference.Create(UserId, CategoryId, level, _now.AddDays(-30));
            typeof(UserCategoryPreference).GetProperty(nameof(UserCategoryPreference.Category))!
                .SetValue(preference, category);
            return preference;
        }

        [Fact]
        public async Task Execute_ExpenseNotFound_ReturnsEarly()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NoCategoryPreference_ReturnsEarly()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns((UserCategoryPreference?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NoFinancialProfile_ReturnsEarly()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Avoid, category);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_SpentOnOutsidePayPeriod_ReturnsEarly()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Avoid, category);
            var profile = CreateProfile(resetDay: 15);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_AvoidPreference_CreatesNotification()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Avoid, category);
            var profile = CreateProfile(resetDay: 1);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._))
                .Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_MinimalPreferenceUnderThreshold_ReturnsEarly()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Minimal, category);
            var profile = CreateProfile(resetDay: 1, monthlyIncome: 10000);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _budgetQueries.GetTotalSpentForCategoryAsync(UserId, CategoryId, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 15), A<CancellationToken>._))
                .Returns(200);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_MinimalPreferenceOverThreshold_CreatesNotification()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Minimal, category);
            var profile = CreateProfile(resetDay: 1, monthlyIncome: 10000);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _budgetQueries.GetTotalSpentForCategoryAsync(UserId, CategoryId, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 15), A<CancellationToken>._))
                .Returns(600);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._))
                .Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_NotificationCreatedButDedupExists_ReturnsEarly()
        {
            var category = CreateCategory();
            var preference = CreatePreference(CategoryPreferenceLevel.Avoid, category);
            var profile = CreateProfile(resetDay: 1);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense(new DateOnly(2026, 8, 10)));
            A.CallTo(() => _categoryPreferenceRepository.FindIncludingCategoryAsync(UserId, CategoryId, A<CancellationToken>._))
                .Returns(preference);
            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._))
                .Returns(true);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
