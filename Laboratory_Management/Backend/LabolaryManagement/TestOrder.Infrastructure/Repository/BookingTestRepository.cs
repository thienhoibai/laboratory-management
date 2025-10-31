using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class BookingTestRepository : GenericRepository<BookingTest>
    {
        public BookingTestRepository(Data.TestOrderDBContext context) : base(context)
        {
        }

        public BookingTestRepository() : base()
        {
        }

        public async Task<IEnumerable<BookingTest>> GetByBookingIdAsync(Guid bookingId)
        {
            return await Task.Run(() => _context.Set<BookingTest>()
                .Where(bt => bt.BookingId == bookingId)
                .ToList());
        }

    }
}
