using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.AppointmentSlots
{
    public class AppointmentSlotDTO
    {
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly TimeBlock { get; set; } 
    }
}
