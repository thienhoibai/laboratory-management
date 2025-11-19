using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Payment
{
    public class PaymentRequestDTO
    {
        public Guid BookingId { get; set; }
        public double? Amount { get; set; }
    }
}
