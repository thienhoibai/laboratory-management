using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Payment
{
    public class PaymentResponseDTO
    {
        public Guid BookingId { get; set; }
        public string? Method { get; set; }
        public double? Amount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Token { get; set; } = string.Empty;
    }
}
