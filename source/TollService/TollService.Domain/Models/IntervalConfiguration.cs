namespace TollService.Domain.Models;

public class IntervalConfiguration
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<TollInterval> Intervals { get; set; } = new HashSet<TollInterval>();
}

public record TollInterval(TimeOnly StartTime, int Fee);