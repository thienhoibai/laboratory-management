using System;
using System.Collections.Generic;
using System.Linq;
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

        
        public async Task AddBookingTestAsync(Guid bookingId, long catalogId)
        {
            
            var exists = _context.Set<BookingTest>()
                .Any(bt => bt.BookingId == bookingId && bt.CatalogId == catalogId);

            if (!exists)
            {
                var bookingTest = new BookingTest
                {
                    BookingId = bookingId
                    
                };

                await _context.Set<BookingTest>().AddAsync(bookingTest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
