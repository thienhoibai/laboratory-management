using Instrument.Domain.Enums;

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
    public RunStatus Status { get; set; } = RunStatus.Running;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
