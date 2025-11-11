namespace Instrument.Application.Results;

public interface IResultGenerator
{
    // return value for each ParameterId (one value per ParameterId per booking)
    Dictionary<int, decimal> GenerateValues(
        IEnumerable<ForInstrumentItemDto> items,
        string? patientSex);
}

// Mirror DTO dùng ở Presentation
public record ForInstrumentItemDto(long TestBookingNo,int CatalogId,int ParameterId,string ParameterName,string? Unit,string? ReferenceRange);
