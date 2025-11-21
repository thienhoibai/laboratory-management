namespace Instrument.Domain.Entities;

public class Instrument
{
    public int InstrumentId { get; set; }
    public string InstrumentCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "ONLINE"; // ONLINE|OFFLINE|FAULT|MAINTENANCE
    public string ReagentStatus { get; set; } = "OK"; // OK|LOW|OUT
    public DateTime? LastHeartbeatAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
