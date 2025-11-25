using Instrument.Domain.Enums;

namespace Instrument.Application.Instruments.DTOs.Responses;

public record InstrumentResponse(
    int InstrumentId,
    string InstrumentCode,
    string Name,
    InstrumentStatus Status,
    ReagentStatus ReagentStatus,
    string? ImageUrl,
    DateTime CreatedAt
);

