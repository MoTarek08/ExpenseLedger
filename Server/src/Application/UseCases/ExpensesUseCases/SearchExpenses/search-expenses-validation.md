# Architecture Decisions

## Search Expenses — Projection & Date Validation

Expenses search uses `IQueryable` composition with a projected read (via `CreateExpenseDtoAsync`). Date range bounds (`From`/`To`) are validated at the FluentValidation layer using `ValidDateOnlyRange()` rather than at the use case layer, consistent with the project's separation of concerns: validators handle format/range, use cases handle business rules.

## Redundant Include Removal

`CreateExpenseDtoAsync` no longer chains `.Include(x => x.Notes)` because the `Select()` projection never materializes the Notes navigation — EF Core ignores includes when the query ends with a projection. Removing it avoids unnecessary SQL joins.
