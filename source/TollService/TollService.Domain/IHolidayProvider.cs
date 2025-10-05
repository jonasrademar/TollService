namespace TollService.Domain;

// Separation of concern; TollService *skulle* kunna ansvara f�r att konfigurera upp
// vilka dagar som �r undantagna f�r tr�ngselskatten; men det b�r oavsett vara konfigurerbart
// snarare �n h�rdkodat. Det �r t�nkbart att konfigueringen sker i anrop mot TollService, 
// men f�r uppgiften s� g�r vi det enkelt f�r oss och l�gger enbart Get operation i providern.

// Sen �r "HolidayProvider" kanske inte klockrent. Vi hade "l�rdagar, helgdagar, dagar f�re helgdag eller under juli m�nad"
// som krav. Jag hittade inget namn jag gillade s� k�rde p� Holiday & HolidayProvider.

public interface IHolidayProvider
{
    Task<IEnumerable<DateOnly>> GetHolidays(int year);
}
