using Instrument.Domain.Enums;

namespace Instrument.Application.Instruments.DTOs.Requests;

public record CreateInstrumentRequest(
    string InstrumentCode,
    string Name,
    InstrumentStatus Status = InstrumentStatus.Online,
    ReagentStatus ReagentStatus = ReagentStatus.OK,
    string? ImagePath = null
);

