using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Infrastructure.Repository
{
    public class BookingRepository : GenericRepository<Booking>
    {
        public BookingRepository(Data.TestOrderDBContext context) : base(context)
        {
        }
        public BookingRepository() : base()
        {
        }

        public async Task<IEnumerable<Booking>?> GetBookingsByPatientIdAsync(long patientId)
        {
            return await _context.Set<Booking>()
                .Where(b => b.PatientId == patientId)
                .ToListAsync();
        }
    }
}
