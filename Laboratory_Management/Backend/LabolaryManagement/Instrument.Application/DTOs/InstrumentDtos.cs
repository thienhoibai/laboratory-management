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
    DateTime? LastHeartbeatAt,
    DateTime CreatedAt
);

public record InstrumentListItem(
    int InstrumentId,
    string InstrumentCode,
    string Name,
    string Status,
    string ReagentStatus,
    DateTime? LastHeartbeatAt
);

// ===== Cartridges =====
public record CartridgeSummaryDto(
    string InstrumentCode,
    int SkuId,
    string SkuCode,
    decimal TotalOnboard,
    decimal? MinLevel,
    string ReagentStatus, // OK|LOW|OUT
    List<CartridgeLotDto> Lots
);

public record CartridgeLotDto(int LotId, string LotNo, decimal QtyRemaining);

public record LoadCartridgeLineDto(int SkuId, int LotId, decimal Qty, decimal? MinLevel);
public record LoadCartridgeRequest(string InstrumentCode, List<LoadCartridgeLineDto> Lines);
public record LoadCartridgeResponse(int LoadId, List<LoadCartridgeLineDto> Lines, string Message);

public record ReturnCartridgeLineDto(int SkuId, int LotId, decimal Qty);
public record ReturnCartridgeRequest(string InstrumentCode, List<ReturnCartridgeLineDto> Lines);
public record ReturnCartridgeResponse(string Message);

// ===== Runs =====
public record StartRunRequest(Guid BookingId, string InstrumentCode);
public record StartRunResponse(int RunId, string Status, string Message);

public record RunDetailDto(
    int RunId,
    Guid BookingId,
    string InstrumentCode,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    List<RunUsageDto> Usages
);

public record RunUsageDto(int SkuId, string SkuCode, int LotId, string LotNo, decimal QtyUsed);

public record CompleteRunRequest(int RunId);
public record CompleteRunResponse(string Status, string Message, List<RunUsageDto> Usages);

public record DropCsvRequest(int RunId);
public record DropCsvResponse(string FilePath, string Message);

// ===== Status =====
public record InstrumentStatusDto(
    string InstrumentCode,
    string Name,
    string Status,
    string ReagentStatus,
    DateTime? LastHeartbeatAt,
    RunDetailDto? CurrentRun
);

// ===== Shortage (thiếu thuốc) =====
public record ShortageDto(int SkuId, string SkuCode, decimal Need, decimal Onboard);
