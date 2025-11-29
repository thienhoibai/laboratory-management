using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Enums
{
    public enum BookingStatusEnum : byte
    {
        Pending = 1,
        Confirmed = 2,
        CheckedIn = 3,
        InProgress = 4,

        Completed = 5,
        Cancelled = 6
        
    }
}
