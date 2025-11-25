using Instrument.Domain.Enums;

namespace Instrument.Application.Instruments.DTOs.Responses;

public record InstrumentStatusDto(
    string InstrumentCode,
    string Name,
    InstrumentStatus Status,
    ReagentStatus ReagentStatus,
    Runs.DTOs.Responses.RunDetailDto? CurrentRun
);

