using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instrument.Application.Statistics.DTOs
{
    public class InstrumentReagentStatisticsDto
    {
        public int TotalInstruments { get; init; }
        public int ReagentFullInstruments { get; init; }
        public int ReagentLowInstruments { get; init; }
        public int ReagentEmptyInstruments { get; init; }
        public DateTime GeneratedAt { get; init; } = DateTime.Now;

    }
}
