namespace Application.Models;

public static class DateConstants
{
    public static DateOnly MinDate => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20));
    public static DateOnly MaxDate => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(20));
}
