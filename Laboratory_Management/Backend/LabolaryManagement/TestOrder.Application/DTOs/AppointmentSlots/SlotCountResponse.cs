using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.AppointmentSlots
{
    public class SlotCountResponse : AppointmentSlotDTO
    {
        public int TotalBookings { get; set; }
        public bool IsFullyBooked { get; set; }
    }
}
