namespace TollService.Domain;

// Separation of concern; TollService *skulle* kunna ansvara för att konfigurera upp
// vilka dagar som är undantagna för trängselskatten; men det bör oavsett vara konfigurerbart
// snarare än hårdkodat. Det är tänkbart att konfigueringen sker i anrop mot TollService, 
// men för uppgiften så gör vi det enkelt för oss och lägger enbart Get operation i providern.

// Sen är "HolidayProvider" kanske inte klockrent. Vi hade "lördagar, helgdagar, dagar före helgdag eller under juli månad"
// som krav. Jag hittade inget namn jag gillade så körde på Holiday & HolidayProvider.

public interface IHolidayProvider
{
    Task<IEnumerable<DateOnly>> GetHolidays(int year);
}
