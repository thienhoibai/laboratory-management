    using Instrument.Domain.Enums;

    namespace Instrument.Domain.Entities;

    public class Instrument
    {
        public int InstrumentId { get; set; }
        public string InstrumentCode { get; set; } = "";
        public string Name { get; set; } = "";
        public InstrumentStatus Status { get; set; } = InstrumentStatus.Online;
        public ReagentStatus ReagentStatus { get; set; } = ReagentStatus.OK;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
