using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Infrastructure.Models.VnPayModels
{
    public class VnPayResponseModel
    {
        public string OrderDescription { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;

        public string ResponseCode { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
