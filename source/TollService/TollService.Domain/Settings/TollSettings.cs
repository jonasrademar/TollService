namespace TollService.Domain.Settings;

// Är allergisk mot hårdkodad konfiguration, om det inte är
// djupt rotad affärslogik som aldrig skulle kunna tänkas ändras.
public class TollSettings
{
    public int DailyCap { get; set; }
}