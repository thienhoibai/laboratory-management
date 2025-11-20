namespace Instrument.Domain.Entities;

public class InstrumentResult
{
    public int InstrumentResultId { get; set; }
    public string InstrumentCode { get; set; } = "";
    public Guid BookingId { get; set; }
    public long TestBookingNo { get; set; }
    public int ParameterId { get; set; }
    public string ParameterName { get; set; } = "";
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
