using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.SpendingGoalsUseCases.Helpers;
using Application.UseCases.SpendingGoalsUseCases.GetSpendingGoalById.Models;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.Notification;
using Domain.Entities.SpendingGoalNamespace;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class CheckGoalAchievementTests
    {
        private readonly ISpendingGoalsRepository _goalsRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IExpensesRepository _expensesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly CheckGoalAchievement _sut;

        private static readonly Guid ExpenseId = Guid.NewGuid();
        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid CategoryId = Guid.NewGuid();
        private readonly DateTimeOffset _now = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
        private readonly DateOnly _spentOn = new DateOnly(2026, 8, 1);

        public CheckGoalAchievementTests()
        {
            _goalsRepository = A.Fake<ISpendingGoalsRepository>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _expensesRepository = A.Fake<IExpensesRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();

            A.CallTo(() => _dateTimeProvider.Now).Returns(_now);

            _sut = new CheckGoalAchievement(
                _goalsRepository, _notificationsRepository, _expensesRepository, _unitOfWork, _dateTimeProvider);
        }

        private Expense CreateExpense() =>
            Expense.CreateManualExpense(UserId, CategoryId, "Test", 100, _spentOn, _now.AddHours(-1));

        private SpendingGoal CreateGoal(decimal minTarget, decimal maxTarget) =>
            SpendingGoal.Create(UserId, null, CategoryId, maxTarget, minTarget, new DateOnly(2026, 7, 1), new DateOnly(2026, 12, 31), _now.AddDays(-30));

        [Fact]
        public async Task Execute_ExpenseNotFound_ReturnsEarly()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NoAffectedGoals_ReturnsEarly()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(CreateExpense());
            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .Returns([]);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_GoalsExistButNoneMeetTarget_NoNotifications()
        {
            var goal = CreateGoal(200, 500);
            var expense = CreateExpense();
            var goalWithSpent = new SpendingGoalWithSpent(goal, 50);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .ReturnsNextFromSequence([goal], []);
            A.CallTo(() => _goalsRepository.GetGoalsWithSpentAsync(A<List<Guid>>._)).Returns([goalWithSpent]);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_GoalMeetsTarget_CreatesNotification()
        {
            var goal = CreateGoal(100, 500);
            var expense = CreateExpense();
            var goalWithSpent = new SpendingGoalWithSpent(goal, 300);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .ReturnsNextFromSequence([goal], []);
            A.CallTo(() => _goalsRepository.GetGoalsWithSpentAsync(A<List<Guid>>._)).Returns([goalWithSpent]);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._)).Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_GoalMeetsTargetButDedupExists_SkipsNotification()
        {
            var goal = CreateGoal(100, 500);
            var expense = CreateExpense();
            var goalWithSpent = new SpendingGoalWithSpent(goal, 300);

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .ReturnsNextFromSequence([goal], []);
            A.CallTo(() => _goalsRepository.GetGoalsWithSpentAsync(A<List<Guid>>._)).Returns([goalWithSpent]);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._)).Returns(true);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_MultipleBatchesPaginated_ProcessesAll()
        {
            var goal1 = CreateGoal(100, 500);
            var goal2 = CreateGoal(50, 200);
            var goal3 = CreateGoal(100, 300);
            var expense = CreateExpense();

            var firstBatch = new List<SpendingGoal> { goal1, goal2 };
            var secondBatch = new List<SpendingGoal> { goal3 };

            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _goalsRepository.FindAffectedByExpenseAsync(A<Guid>._, A<Guid>._, A<DateOnly>._, A<Guid>._, A<int>._))
                .ReturnsNextFromSequence(firstBatch, secondBatch, []);
            A.CallTo(() => _goalsRepository.GetGoalsWithSpentAsync(A<List<Guid>>._))
                .ReturnsNextFromSequence(
                    [new SpendingGoalWithSpent(goal1, 300), new SpendingGoalWithSpent(goal2, 30)],
                    [new SpendingGoalWithSpent(goal3, 200)]);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._)).Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappened(2, Times.Exactly);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappened(2, Times.Exactly);
        }
    }
}
