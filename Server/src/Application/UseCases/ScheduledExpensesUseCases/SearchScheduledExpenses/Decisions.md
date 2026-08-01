# Search Scheduled Expenses — Key Decisions

## Endpoint
`GET /api/ScheduledExpenses/search` — follows the exact pattern of the existing `Expenses/search` and `Notifications/search` endpoints.

## Query Parameters
Inherits `PaginationParameters` (`PageNumber`, `PageSize`) That are self-validated

## Sorting
SortBy is validated against `ScheduledExpensesSortOptions.All` via FluentValidation. The use case priortize sorting ASC with NextDueOn and falls back to that,
If other sort key was provided, the second sort key will also be NextDueOn since this is what serves the UX most

## Repository Additions
Two new methods on `IExpensesRepository`:
- `GetAllScheduledForUserAsQuery(Guid userId)` — returns `IQueryable<ScheduledExpense>` pre-filtered by user.
- `GetScheduledExpenseDtoAsync(IQueryable<ScheduledExpense> query)` — materializes the query with `.Include(Category, SubCategory)` and `Select` projection to `ScheduledExpenseDto`.

## Projection
DTO conversion uses a `Select` expression directly on the query (not `.AsScheduledExpenseDto()` on loaded entities), matching the `ExpenseDto` projection pattern.

## Error Handling
The use case has no failure paths (same as `SearchExpensesUseCase`). The controller does not check `result.IsFailure`.
