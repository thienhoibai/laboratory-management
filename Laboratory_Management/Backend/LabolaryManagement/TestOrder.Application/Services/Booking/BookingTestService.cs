using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services.Booking
{
    internal class BookingTestService
    {
        private readonly BookingTestRepository _bookingTestRepository;

        public BookingTestService(BookingTestRepository bookingTestRepository)
        {
            _bookingTestRepository = bookingTestRepository;
        }

        public BookingTestService()
        {
            _bookingTestRepository = new BookingTestRepository();
        }

        public async Task<IEnumerable<BookingTest>> GetBookingTestsByBookingIdAsync(long bookingId)
        {
            return await _bookingTestRepository.GetByBookingIdAsync(bookingId);
        }

    }
}
