using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.InstrumentBridge
{

// ===== GET /for-instrument =====
public record ForInstrumentItemDto(
    long TestBookingNo,
    int CatalogId,
    int ParameterId,
    string ParameterName,
    string? Unit,
    decimal? RefMin,
    decimal? RefMax
);

public record DuplicateGroup(
    int ParameterId,
    List<long> TestBookingNos
);

public record ForInstrumentResponse(
    Guid BookingId,
    byte Status,
    string? PatientName,
    List<ForInstrumentItemDto> Items,
    List<DuplicateGroup> DuplicateGroups
);

// ===== POST /results =====
public record IngestItem(
    long TestBookingNo,
    int ParameterId,
    string ParameterName,
    decimal Value,
    string? Unit,
    decimal? RefMin,
    decimal? RefMax
);

public record IngestRequest(
    string InstrumentCode,
    DateTimeOffset MeasuredAt,
    List<IngestItem> Items
);

public record IngestResponse(
    int Accepted,
    int Rejected,
    bool Completed,
    List<MissingPair> MissingPairs
);

public record MissingPair(
    long TestBookingNo,
    int ParameterId
);

// ===== GET /expected =====
public record ExpectedResponse(
    Guid BookingId,
    int ExpectedPairs,
    int ExistingPairs,
    bool Completed,
    List<MissingPair> MissingPairs
);

}
