using Microsoft.AspNetCore.Http;
using Instrument.Domain.Enums;
namespace Instrument.Application.Instruments.DTOs.Requests
{
    

    public class InstrumentCreateHttpRequest
    {
        public string InstrumentCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public InstrumentStatus Status { get; set; } = InstrumentStatus.Online;
        public ReagentStatus ReagentStatus { get; set; } = ReagentStatus.OK;
        public IFormFile? Image { get; set; }

        
    }
}

