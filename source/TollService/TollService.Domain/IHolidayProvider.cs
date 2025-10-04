namespace TollService.Domain;

public interface IHolidayProvider
{
    Task<IEnumerable<DateOnly>> GetHolidays(int year);
}
