using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Application.UseCases.NotificationsUseCases.MarkNotificationAsRead;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotificationsUseCases.MarkNotificationAsRead
{
    public class MarkNotificationAsReadUseCaseTests
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<MarkNotificationAsReadUseCase> _logger;
        private readonly MarkNotificationAsReadUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid NotificationId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public MarkNotificationAsReadUseCaseTests()
        {
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<MarkNotificationAsReadUseCase>>();
            _sut = new MarkNotificationAsReadUseCase(_notificationsRepository, _unitOfWork, _dateTimeProvider, _logger);
        }

        [Fact]
        public async Task Execute_WhenUnreadNotification_ShouldSetReadAt()
        {
            var now = DateTimeOffset.UtcNow;
            var notification = Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(now);

            var result = await _sut.Execute(UserId, NotificationId, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(now, notification.ReadAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_WhenAlreadyRead_ShouldReturnSuccessIdempotent()
        {
            var now = DateTimeOffset.UtcNow;
            var notification = Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);
            notification.MarkAsRead(now.AddHours(-1));

            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(now.AddHours(-1), notification.ReadAt);
            A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenNotificationNotFound_ShouldReturnNotFound()
        {
            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns((Notification?)null);

            var result = await _sut.Execute(UserId, NotificationId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNotificationNotOwned_ShouldReturnNotFound()
        {
            var notification = Notification.BudgetWentNegative(OtherUserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, default);

            Assert.True(result.IsFailure);
            Assert.Equal(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND, result.Error!.Code);
        }
    }
}
