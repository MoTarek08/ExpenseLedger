using Application.Interfaces.DateTimeProvider;
using Application.Interfaces.Repositories;
using Application.Interfaces.RepositoriesNamespace;
using Application.Models;
using Application.Models.Result;
using Application.UseCases.BudgetUseCases.Helpers;
using Application.UseCases.NotificationsUseCases.Models;
using Domain.Entities.Notification;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationsUseCases.GetCurrentPeriodNotifications
{
    public class GetCurrentPeriodNotificationsUseCase
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IDateProvider _dateTimeProvider;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly ILogger<GetCurrentPeriodNotificationsUseCase> _logger;

        public GetCurrentPeriodNotificationsUseCase(
            IUsersRepository usersRepository,
            INotificationsRepository notificationsRepository,
            IDateProvider dateTimeProvider,
            ILogger<GetCurrentPeriodNotificationsUseCase> logger)
        {
            _usersRepository = usersRepository;
            _dateTimeProvider = dateTimeProvider;
            _notificationsRepository = notificationsRepository;
            _logger = logger;
        }

        public async Task<Result<List<NotificationDto>>> Execute(Guid userId, PaginationParameters paginationParams, CancellationToken cancellationToken)
        {
            List<NotificationDto> dtos = new();
            var financialProfile = await _usersRepository.GetFinancialProfileByUserIdAsync(userId, cancellationToken);
            if (financialProfile is null)
            {
                _logger.LogInformation("User has no financial profile {UserId}", userId);
                return Result<List<NotificationDto>>.Success(dtos);
            }

            var today = DateOnly.FromDateTime(_dateTimeProvider.Now.UtcDateTime);
            var lastPayDay = BudgetComputingHelpers.GetLastPayDay(financialProfile.ResetDay, today);

            var all = _notificationsRepository.GetVisibleInPeriodQuery(userId, lastPayDay, today);
            var paginatedResult = await _notificationsRepository
                .ToListAsync(
                all
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize),
                cancellationToken);

            dtos = paginatedResult.Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.Reason,
                n.Type,
                n.Title,
                n.Body,
                n.ReadAt,
                n.ExpenseId,
                n.SpendingGoalId,
                n.ScheduledExpenseId,
                n.CategoryId)).ToList();

            _logger.LogInformation("Retrieved {NotificationsCount} notifications for current period {UserId}", dtos.Count, userId);

            return Result<List<NotificationDto>>.Success(dtos);
        }
    }
}
