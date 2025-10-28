using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services.Booking
{
    public class BookingService
    {
        private readonly BookingTestService _bookingTestService;
        private readonly BookingRepository _bookingRepository;

        public BookingService(BookingRepository bookingRepository)
        {
            _bookingTestService = new BookingTestService();
            _bookingRepository = bookingRepository;
        }


        

    }
}
