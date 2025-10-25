using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Domain
{
    public class TimeBlock
    {
        public int TimeBlockId { get; }
        public TimeOnly Time { get; }
        public TimeBlock(int timeBlockId, TimeOnly time)
        {
            TimeBlockId = timeBlockId;
            Time = time;
        }

        public TimeBlock()
        {

        }

        public bool checkAvailability(DateTime appointmentDateTime)
        {
            return appointmentDateTime.TimeOfDay == Time.ToTimeSpan();
        }

        public override string ToString()
        {
            return Time.ToString("HH:mm");
        }
    }
}
