using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.UseCases.NotificationsUseCases.GetCurrentPeriodNotifications;
using Domain.Entities.DomainEnums;
using Domain.Entities.Notification;
using Domain.Entities.UserFinancialProfileNamespace;
using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.UntiTests.UseCases.NotificationsUseCases.GetCurrentPeriodNotifications
{
    public class GetCurrentPeriodNotificationsUseCaseTests
    {
        private readonly IUsersRepository _usersRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly ILogger<GetCurrentPeriodNotificationsUseCase> _logger;
        private readonly GetCurrentPeriodNotificationsUseCase _sut;

        private static readonly Guid UserId = Guid.Parse("bff0aee8-b680-4546-8e9e-f253cd2d5930");

        public GetCurrentPeriodNotificationsUseCaseTests()
        {
            _usersRepository = A.Fake<IUsersRepository>();
            _notificationsRepository = A.Fake<INotificationsRepository>();
            _dateTimeProvider = A.Fake<IDateProvider>();
            _logger = A.Fake<ILogger<GetCurrentPeriodNotificationsUseCase>>();
            _sut = new GetCurrentPeriodNotificationsUseCase(_usersRepository, _notificationsRepository, _dateTimeProvider, _logger);
        }

        [Fact]
        public async Task Execute_WhenNoFinancialProfile_ShouldReturnEmptyList()
        {
            var pagination = new PaginationParameters { PageNumber = 1, PageSize = 20 };

            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns((UserFinancialProfile?)null);

            var result = await _sut.Execute(UserId, pagination, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!);
            A.CallTo(() => _notificationsRepository.GetVisibleInPeriodQuery(A<Guid>._, A<DateOnly>._, A<DateOnly>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Execute_WhenHasFinancialProfile_ShouldReturnNotifications()
        {
            var now = new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero);
            var profile = UserFinancialProfile.Create(UserId, 5000, 1, now);
            var pagination = new PaginationParameters { PageNumber = 1, PageSize = 20 };
            var notifications = new List<Notification>
            {
                Notification.BudgetWentNegative(UserId, Guid.NewGuid(), -500, new DateOnly(2026, 7, 1), now)
            };

            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(now);
            A.CallTo(() => _notificationsRepository.GetVisibleInPeriodQuery(UserId, A<DateOnly>._, A<DateOnly>._))
                .Returns(notifications.AsQueryable());
            A.CallTo(() => _notificationsRepository.ToListAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .Returns(notifications);

            var result = await _sut.Execute(UserId, pagination, default);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            A.CallTo(() => _notificationsRepository.ToListAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Execute_ShouldApplyPagination()
        {
            var now = new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero);
            var profile = UserFinancialProfile.Create(UserId, 5000, 1, now);
            var pagination = new PaginationParameters { PageNumber = 2, PageSize = 10 };
            var notifications = new List<Notification>();

            A.CallTo(() => _usersRepository.GetFinancialProfileByUserIdAsync(UserId, A<CancellationToken>._))
                .Returns(profile);
            A.CallTo(() => _dateTimeProvider.Now)
                .Returns(now);
            A.CallTo(() => _notificationsRepository.GetVisibleInPeriodQuery(UserId, A<DateOnly>._, A<DateOnly>._))
                .Returns(notifications.AsQueryable());
            A.CallTo(() => _notificationsRepository.ToListAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .ReturnsLazily((IQueryable<Notification> query, CancellationToken _) =>
                    Task.FromResult(query.Skip(10).Take(10).ToList()));

            var result = await _sut.Execute(UserId, pagination, default);

            Assert.True(result.IsSuccess);
            A.CallTo(() => _notificationsRepository.ToListAsync(A<IQueryable<Notification>>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}
