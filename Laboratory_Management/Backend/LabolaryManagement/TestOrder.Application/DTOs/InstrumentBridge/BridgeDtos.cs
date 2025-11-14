using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.InstrumentBridge
{
    // Item đã được gộp theo ParameterId, nhưng vẫn liệt kê đầy đủ các TestBookingNo/CatalogIds
    public record ForInstrumentItem(
        int ParameterId,
        string ParameterName,
        string? Unit,
        decimal? RefMin,
        decimal? RefMax,
        List<long> TestBookingNos,
        List<int> CatalogIds
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
