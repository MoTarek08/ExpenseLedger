using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.ExpenseNamespace;
using Domain.Entities.Notification;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class CreateScheduledExpenseGeneratedNotificationTests
    {
        private readonly IExpensesRepository _expensesRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CreateScheduledExpenseGeneratedNotification _sut;

        private static readonly Guid ExpenseId = Guid.NewGuid();
        private static readonly Guid UserId = Guid.NewGuid();
        private static readonly Guid ScheduledExpenseId = Guid.NewGuid();
        private static readonly Guid CategoryId = Guid.NewGuid();
        private readonly DateTimeOffset _now = new DateTimeOffset(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);

        public CreateScheduledExpenseGeneratedNotificationTests()
        {
            _expensesRepository = A.Fake<IExpensesRepository>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            A.CallTo(() => _dateTimeProvider.Now).Returns(_now);

            _sut = new CreateScheduledExpenseGeneratedNotification(
                _expensesRepository, _notificationsRepository, _dateTimeProvider, _unitOfWork);
        }

        private Expense CreateScheduledExpense() =>
            Expense.CreateManualExpense(UserId, CategoryId, "Rent", 1000, new DateOnly(2026, 8, 1), _now.AddHours(-1))
                .LinkToScheduledExpense(ScheduledExpenseId, new DateOnly(2026, 8, 1));

        [Fact]
        public async Task Execute_ExpenseNotFound_ReturnsEarly()
        {
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Expense?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NotScheduledExpense_ReturnsEarly()
        {
            var expense = Expense.CreateManualExpense(UserId, CategoryId, "Manual", 100, new DateOnly(2026, 8, 1), _now.AddHours(-1));
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_ValidScheduledExpense_CreatesNotification()
        {
            var expense = CreateScheduledExpense();
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(A<Guid>._, A<string>._, A<CancellationToken>._)).Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_DedupAlreadyExists_ReturnsEarly()
        {
            var expense = CreateScheduledExpense();
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(UserId, A<string>._, A<CancellationToken>._)).Returns(true);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_DuplicateKeyOnSave_CaughtSilently()
        {
            var expense = CreateScheduledExpense();
            A.CallTo(() => _expensesRepository.FindAsync(ExpenseId, A<CancellationToken>._)).Returns(expense);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(A<Guid>._, A<string>._, A<CancellationToken>._)).Returns(false);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).Throws<NotificationDeuplicationKeyAlreadyExists>();

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustHaveHappenedOnceExactly();
        }
    }
}
