using Application.ErrorNamespace;
using Application.ErrorNamespace.ErrorCodesNamespace;
using Application.Interfaces.Repositories;
using Application.UseCases.NotificationsUseCases.GetNotificationById;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotificationsUseCases.GetNotificationById
{
    public class GetNotificationByIdUseCaseTests
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly ILogger<GetNotificationByIdUseCase> _logger;
        private readonly GetNotificationByIdUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");
        private static readonly Guid NotificationId = Guid.Parse("d7a8f3b2-1c4e-4f6a-9b0c-5d7e2f1a3b4c");
        private static readonly Guid OtherUserId = Guid.Parse("ab538fed-9005-4e69-ba7d-d9789f3382f3");

        public GetNotificationByIdUseCaseTests()
        {
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _logger = A.Fake<ILogger<GetNotificationByIdUseCase>>();
            _sut = new GetNotificationByIdUseCase(_notificationsRepository, _logger);
        }

        [Fact]
        public async Task Execute_WhenNotificationFoundAndOwned_ShouldReturnDto()
        {
            var notification = Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(notification.Id, result.Data!.Id);
        }

        [Fact]
        public async Task Execute_WhenNotificationNotFound_ShouldReturnNotFound()
        {
            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns((Notification?)null);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND, result.Error!.Code);
        }

        [Fact]
        public async Task Execute_WhenNotificationNotOwned_ShouldReturnNotFound()
        {
            var notification = Notification.BudgetWentNegative(OtherUserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), DateTimeOffset.UtcNow);

            A.CallTo(() => _notificationsRepository.FindVisibleAsync(NotificationId, A<CancellationToken>._))
                .Returns(notification);

            var result = await _sut.Execute(UserId, NotificationId, TestContext.Current.CancellationToken);

            Assert.True(result.IsFailure);
            Assert.Equal(NotificationsErrorCodes.NOTIFICATION_NOT_FOUND, result.Error!.Code);
        }
    }
}
