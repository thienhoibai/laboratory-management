namespace Instrument.Application.DTOs;

// ===== Instrument CRUD =====
public record CreateInstrumentRequest(
    string InstrumentCode,
    string Name,
    string? Status = "ONLINE"
);

public record UpdateInstrumentRequest(
    string? Name,
    string? Status,
    string? ReagentStatus
);

public record InstrumentResponse(
    int InstrumentId,
    string InstrumentCode,
    string Name,
    string Status,
    string ReagentStatus,
    DateTime CreatedAt
);

public record InstrumentListItem(
    int InstrumentId,
    string InstrumentCode,
    string Name,
    string Status,
    string ReagentStatus
);

// ===== Runs =====
public record StartRunRequest(Guid BookingId, string InstrumentCode);
public record StartRunResponse(int RunId, string Status, string Message, int ResultCount);

public record RunDetailDto(
    int RunId,
    Guid BookingId,
    string InstrumentCode,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt
);

// ===== Status =====
public record InstrumentStatusDto(
    string InstrumentCode,
    string Name,
    string Status,
    string ReagentStatus,
    RunDetailDto? CurrentRun
);
