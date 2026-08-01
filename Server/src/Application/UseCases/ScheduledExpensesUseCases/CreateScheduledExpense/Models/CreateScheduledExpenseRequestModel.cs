using Domain.Entities.DomainEnums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UseCases.ScheduledExpensesUseCases.CreateScheduledExpense.Models
{
    public sealed record CreateScheduledExpenseRequestModel(
        string? Title,
        decimal Amount,
        Guid CategoryId,
        Guid? SubCategoryId,
        CadenceInterval Cadence,
        DateOnly FirstDueOn);
}
