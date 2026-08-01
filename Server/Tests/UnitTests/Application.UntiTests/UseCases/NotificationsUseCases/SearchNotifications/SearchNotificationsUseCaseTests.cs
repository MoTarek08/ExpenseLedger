using Application.ApplicationConstantsNamesapce;
using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.UseCases.NotificationsUseCases.SearchNotifications;
using Application.UseCases.NotificationsUseCases.Models;
using Application.UseCases.NotificationsUseCases.SearchNotifications.Models;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotificationsUseCases.SearchNotifications
{
    public class SearchNotificationsUseCaseTests
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<SearchNotificationsUseCase> _logger;
        private readonly SearchNotificationsUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");

        public SearchNotificationsUseCaseTests()
        {
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<SearchNotificationsUseCase>>();
            _sut = new SearchNotificationsUseCase(_notificationsRepository, _dateTimeProvider, _logger);
        }

        [Fact]
        public async Task Execute_WhenNoFilters_ShouldReturnPaginatedResult()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null);
            var allNotifications = Enumerable.Range(0, queryParams.PageSize * 2)
                .Select(_ => Notification.BudgetWentNegative(
                    UserId, Guid.NewGuid(), -100, new DateOnly(2026, 7, 21), DateTimeOffset.UtcNow))
                .ToList();

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(allNotifications.AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(
                    A<IQueryable<Notification>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Notification> query, CancellationToken _) =>
                {
                    var paginated = query.ToList();
                    return paginated.Select(n => CreateDto(n.Type, n.ReadAt)).ToList();
                });

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(queryParams.PageSize, result.Data!.Count);
        }

        [Fact]
        public async Task Execute_WhenAvailableRowsLessThanPageSize_ShouldReturnAllRows()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null);
            var allNotifications = Enumerable.Range(0, 5)
                .Select(_ => Notification.BudgetWentNegative(
                    UserId, Guid.NewGuid(), -100, new DateOnly(2026, 7, 21), DateTimeOffset.UtcNow))
                .ToList();

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(allNotifications.AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(
                    A<IQueryable<Notification>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Notification> query, CancellationToken _) =>
                {
                    var paginated = query.ToList();
                    return paginated.Select(n => CreateDto(n.Type, n.ReadAt)).ToList();
                });

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Data!.Count);
        }

        [Fact]
        public async Task Execute_WhenFilterByReadOnly_ShouldReturnOnlyRead()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null, ReadOnly: true);
            var dtos = new List<NotificationDto>
            {
                CreateDto(NotificationType.Warning, DateTimeOffset.UtcNow)
            };

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(new List<Notification>().AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .Returns(dtos);

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task Execute_WhenFilterByUnreadOnly_ShouldReturnOnlyUnread()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null, UnreadOnly: true);
            var dtos = new List<NotificationDto>
            {
                CreateDto(NotificationType.Warning, null)
            };

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(new List<Notification>().AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .Returns(dtos);

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task Execute_WhenReadOnlyAndUnreadOnlyBothTrue_ShouldReturnEmpty()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null, UnreadOnly: true, ReadOnly: true);

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(new List<Notification>().AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .Returns(new List<NotificationDto>());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task Execute_ShouldApplyPagination()
        {
            var queryParams = new SearchNotificationsQueryParameters(null, null, null, SortBy: "CreationDate", SortOrder: "Descending");

            A.CallTo(() => _notificationsRepository.GetAllVisibleForUserQuery(UserId))
                .Returns(new List<Notification>().AsQueryable());
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .Returns(new List<NotificationDto>());

            var result = await _sut.Execute(UserId, queryParams, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _notificationsRepository.GetNotificationDtoAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private static NotificationDto CreateDto(NotificationType type, DateTimeOffset? readAt)
        {
            return new NotificationDto(
                Guid.NewGuid(),
                UserId,
                NotificationReason.BudgetWentNegative,
                type,
                "Title",
                "Body",
                readAt,
                null, null, null, null);
        }
    }
}
