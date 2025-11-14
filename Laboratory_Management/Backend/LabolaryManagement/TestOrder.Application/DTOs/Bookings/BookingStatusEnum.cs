using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Bookings
{
    public enum BookingStatusEnum : byte
    {
        Pending = 1,
        Confirmed = 2,
        CheckedIn = 3,
        InProgress = 4,

        ReadyForInstrument = 5,

        Completed = 6,
        Cancelled = 7
    }
}
