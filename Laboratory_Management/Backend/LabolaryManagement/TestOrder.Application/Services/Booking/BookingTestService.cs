using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services.Booking
{
    public class BookingTestService
    {
        private readonly BookingTestRepository _bookingTestRepository;

        public BookingTestService(BookingTestRepository bookingTestRepository)
        {
            _bookingTestRepository = bookingTestRepository;
        }

        public async Task<int> AddBookingTestAsync(long bookingId, long testId)
        {
           
        }
    }
}
