using Application.Exceptions.StorageExceptions.UniqueViolation;
using Application.Interfaces.Repositories;
using Application.Interfaces.SharedServices;
using Application.Interfaces.UnitOfWork;
using Domain.Entities.Notification;
using FakeItEasy;
using Infrastructure.BackgroundJobs.BackgroundJobs.AfterExpenseCreationJobs;

namespace UnitTests.Infrastructure.UnitTests.BackgroundJobs
{
    public class CheckBudgetAfterExpenseCreationJobTests
    {
        private readonly ICheckBudgetStateService _checkBudgetState;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CheckBudgetAfterExpenseCreationJob _sut;

        private static readonly Guid ExpenseId = Guid.NewGuid();

        public CheckBudgetAfterExpenseCreationJobTests()
        {
            _checkBudgetState = A.Fake<ICheckBudgetStateService>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();

            _sut = new CheckBudgetAfterExpenseCreationJob(
                _checkBudgetState, _notificationsRepository, _unitOfWork);
        }

        [Fact]
        public async Task Execute_NoBudgetNotification_ReturnsEarly()
        {
            A.CallTo(() => _checkBudgetState.EvaluateAsync(ExpenseId, A<CancellationToken>._))
                .Returns((Notification?)null);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_NotificationCreated_SavesSuccessfully()
        {
            var notification = Notification.BudgetWentNegative(Guid.NewGuid(), ExpenseId, -10, new DateOnly(2026, 8, 1), DateTimeOffset.UtcNow);
            A.CallTo(() => _checkBudgetState.EvaluateAsync(ExpenseId, A<CancellationToken>._)).Returns(notification);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(A<Guid>._, A<string>._, A<CancellationToken>._)).Returns(false);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(notification)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_DedupAlreadyExists_ReturnsEarly()
        {
            var notification = Notification.BudgetWentNegative(Guid.NewGuid(), ExpenseId, -10, new DateOnly(2026, 8, 1), DateTimeOffset.UtcNow);
            A.CallTo(() => _checkBudgetState.EvaluateAsync(ExpenseId, A<CancellationToken>._)).Returns(notification);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(notification.UserId, notification.DedupKey, A<CancellationToken>._)).Returns(true);

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(A<Notification>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_DuplicateKeyOnSave_CaughtSilently()
        {
            var notification = Notification.BudgetWentNegative(Guid.NewGuid(), ExpenseId, -10, new DateOnly(2026, 8, 1), DateTimeOffset.UtcNow);
            A.CallTo(() => _checkBudgetState.EvaluateAsync(ExpenseId, A<CancellationToken>._)).Returns(notification);
            A.CallTo(() => _notificationsRepository.ExistsByDedupKeyAsync(A<Guid>._, A<string>._, A<CancellationToken>._)).Returns(false);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).Throws<NotificationDeuplicationKeyAlreadyExists>();

            await _sut.Execute(ExpenseId);

            A.CallTo(() => _notificationsRepository.Add(notification)).MustHaveHappenedOnceExactly();
        }
    }
}
