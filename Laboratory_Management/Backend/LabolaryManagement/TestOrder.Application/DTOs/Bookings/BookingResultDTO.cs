using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Bookings
{
    public class BookingResultDTO
    {
        public string? Message { get; set; }
        public Guid? BookingId { get; set; }
    }
}
