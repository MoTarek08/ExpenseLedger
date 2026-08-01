namespace Application.UseCases.BudgetUseCases.Helpers
{
    public static class BudgetComputingHelpers
    {
        public static DateOnly GetLastPayDay(int resetDay, DateOnly today)
        {
            DateOnly payDay;

            if (resetDay == today.Day)
            {
                payDay = today;
            }
            else if (resetDay > today.Day)
            {
                var previousMonth = today.AddMonths(-1);

                payDay = new DateOnly(
                    previousMonth.Year,
                    previousMonth.Month,
                    GetValidDay(previousMonth.Year, previousMonth.Month, resetDay));
            }
            else
            {
                payDay = new DateOnly(
                    today.Year,
                    today.Month,
                    GetValidDay(today.Year, today.Month, resetDay));
            }

            return payDay;
        }

        private static int GetValidDay(int year, int month, int desiredDay)
        {
            return Math.Min(
                desiredDay,
                DateTime.DaysInMonth(year, month));
        }
    }
}
