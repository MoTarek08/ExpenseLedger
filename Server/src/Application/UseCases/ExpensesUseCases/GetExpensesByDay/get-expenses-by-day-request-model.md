# Architecture Decisions

## GetExpensesByDay — Request Model & Validator

The `DateOnly day` parameter was promoted to a `GetExpensesByDayRequestModel` record so that FluentValidation can apply `ValidDateOnlyRange()` via `[FromServices]` injection. This keeps the controller thin and consistent with every other endpoint in the project that validates input — no raw parameter validation in use cases or controllers.

## GetExpensesByDay — Add logging

Added `ILogger<GetExpensesByDayUseCase>` to log the user ID and day being queried, providing observability without altering the endpoint's contract.
