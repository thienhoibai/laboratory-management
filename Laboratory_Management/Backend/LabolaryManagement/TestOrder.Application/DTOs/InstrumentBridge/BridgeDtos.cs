using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.InstrumentBridge
{
    

    public record ForInstrumentItem(
        long TestBookingNo,
        int CatalogId,
        int ParameterId,
        string ParameterName,
        string? Unit,
        string? ReferenceRange
    );

    public record ForInstrumentResponse(
        Guid BookingId,
        string? PatientName,
        string? PatientSex,               // "M" / "F" / null
        List<ForInstrumentItem> Items
    );

    public record ResultItem(long TestBookingNo, int ParameterId, decimal Value);

    public record PostResultsRequest(
        Guid BookingId,
        string InstrumentCode,
        DateTimeOffset MeasuredAt,
        List<ResultItem> Results
    );

}
