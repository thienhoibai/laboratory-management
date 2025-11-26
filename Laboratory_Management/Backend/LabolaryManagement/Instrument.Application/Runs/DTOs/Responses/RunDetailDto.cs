using Instrument.Domain.Enums;

namespace Instrument.Application.Runs.DTOs.Responses;

public record RunDetailDto(
    int RunId,
    Guid BookingId,
    string InstrumentCode,
    RunStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt
);

