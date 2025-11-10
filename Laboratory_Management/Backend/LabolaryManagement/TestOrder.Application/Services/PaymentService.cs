using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services
{
    public class PaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        public PaymentService(PaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<bool> IsPaymentCompletedAsync(Guid bookingId)
        {
            var payment = await _paymentRepository.GetByBookingIdAsync(bookingId);
            return payment != null && payment.Status == "Completed";
        }
    }
}
