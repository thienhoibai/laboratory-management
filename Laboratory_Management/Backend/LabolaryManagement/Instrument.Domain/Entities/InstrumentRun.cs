namespace Instrument.Domain.Entities;

/// <summary>
/// InstrumentRun - Lịch sử run xét nghiệm
/// MINIMAL VERSION - Không có Usages (đã xóa InstrumentRunUsage)
/// </summary>
public class InstrumentRun
{
    public int RunId { get; set; }
    public Guid BookingId { get; set; }
    public string InstrumentCode { get; set; } = string.Empty;
    public string Status { get; set; } = "RUNNING"; // RUNNING|COMPLETED|FAILED
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
