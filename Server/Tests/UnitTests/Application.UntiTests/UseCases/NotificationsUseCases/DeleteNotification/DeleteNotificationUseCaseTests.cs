using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotificationsUseCases.DeleteNotification;
using Domain.Entities.Notification;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotificationsUseCases.DeleteNotification
{
    public class DeleteNotificationUseCaseTests
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<DeleteNotificationUseCase> _logger;
        private readonly DeleteNotificationUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid NotificationId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public DeleteNotificationUseCaseTests()
        {
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<DeleteNotificationUseCase>>();
            _sut = new DeleteNotificationUseCase(_notificationsRepository, _unitOfWork, _dateTimeProvider, _logger);
        }

        [Fact]
        public async Task Execute_WhenNotificationExists_ShouldSetDeletedAt()
        {
            var now = DateTimeOffset.UtcNow;
            var notification = Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(now, notification.DeletedAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenAlreadyDeleted_ShouldReturnSuccessIdempotent()
        {
            var now = DateTimeOffset.UtcNow;
            var notification = Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            notification.MarkAsDeleted(now.AddHours(-2));

            A.CallTo(() => _notificationsRepository.FindAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(now.AddHours(-2), notification.DeletedAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenNotificationNotFound_ShouldReturnSuccessIdempotent()
        {
            A.CallTo(() => _notificationsRepository.FindAsync(NotificationId, A<CancellationToken>._))
                .Returns((Notification?)null);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenNotificationNotOwned_ShouldReturnFailure()
        {
            var notification = Notification.BudgetWentNegative(OtherUserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
