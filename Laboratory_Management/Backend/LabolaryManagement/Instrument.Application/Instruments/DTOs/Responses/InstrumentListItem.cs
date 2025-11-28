using Instrument.Domain.Enums;

namespace Instrument.Application.Instruments.DTOs.Responses;

public record InstrumentListItem(
    int InstrumentId,
    string InstrumentCode,
    string Name,
    InstrumentStatus Status,
    ReagentStatus ReagentStatus,
    string? ImagePath
);

