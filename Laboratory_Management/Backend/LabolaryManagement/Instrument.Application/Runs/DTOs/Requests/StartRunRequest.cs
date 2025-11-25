namespace Instrument.Application.Runs.DTOs.Requests;

public record StartRunRequest(Guid BookingId, string InstrumentCode);

