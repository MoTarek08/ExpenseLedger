using Application.UseCases.NotificationsUseCases.SearchNotifications.Models;
using FluentValidation;
using Host.Validation.ValidatorsNamespace.ValidationExtenstionsNamespace;
using static Application.ApplicationConstantsNamesapce.ApplicationConstants;

namespace Host.Validation.Validators
{
    public sealed class SearchNotificationsQueryParametersValidator
           : PaginationParametersValidator<SearchNotificationsQueryParameters>
    {
        public SearchNotificationsQueryParametersValidator()
        {
            When(x => x.NotificationType is not null, () =>
            {
                RuleFor(x => x.NotificationType)
                    .IsInEnum()
                    .WithMessage("Invalid notification type");
            });

            When(x => x.From.HasValue && x.To.HasValue, () =>
            {
                RuleFor(x => x.To)
                    .GreaterThanOrEqualTo(x => x.From!.Value)
                    .WithMessage("Invalid date range");
            });

            When(x => x.From.HasValue, () =>
            {
                RuleFor(x => x.From!.Value)
                    .ValidDateOnlyRange();
            });

            When(x => x.To.HasValue, () =>
            {
                RuleFor(x => x.To!.Value)
                    .ValidDateOnlyRange();
            });

            RuleFor(x => x.SortBy)
                .Must(v => NotificationsSortOptions.All.Contains(v, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"SortBy must be one of: {string.Join(", ", NotificationsSortOptions.All)}.");

            RuleFor(x => x.SortOrder)
                .ValidSortOrder();

            RuleFor(x => x)
                .Must(x => !(x.ReadOnly && x.UnreadOnly))
                .WithMessage("inconsistent filteration");

        }
    }
}
