using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Payment
{
    public class UpdatePaymentDTO
    {
       
        public string? Method { get; set; }
        public byte? Status { get; set; }
        public DateTime? PaidAt { get; set; }

        public decimal? Amount { get; set; }

    }
}
