using Instrument.Domain.Enums;

namespace Instrument.Application.Instruments.DTOs.Requests;

public record UpdateInstrumentRequest(
    string? Name,
    InstrumentStatus? Status,
    ReagentStatus? ReagentStatus,
    string? ImageUrl
);

