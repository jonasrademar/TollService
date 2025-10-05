namespace TollService.Domain.Models;

// Id & CreatedAt för att kunna ha någon form av idempotency på sikt är tanken.
// Jag tänker mig att man bör peristera trängselskatten snarare än alltid beräkna
// på plats utifrån senaste inställningarna. Antingen med referens till konfiguration
// Id, eller tidpunkten då den genererades.
public class IntervalConfiguration
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<TollInterval> Intervals { get; set; } = new HashSet<TollInterval>();
}

public record TollInterval(TimeOnly StartTime, int Fee);