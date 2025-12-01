using Instrument.Domain.Enums;

namespace Instrument.Presentation.Controllers
{
    public class UpdateInstrumentHttpRequest
    {
        public string Name { get; set; } = null!;
        public InstrumentStatus Status { get; set; } = InstrumentStatus.Online;
        public ReagentStatus ReagentStatus { get; set; } = ReagentStatus.OK;
        public IFormFile? Image { get; set; }
    }
}
