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

        public async Task<List<Booking>?> GetBookingsByPatientIdAsync(long patientId)
        {
            return await Task.Run(() => _context.Set<Booking>()
                .Where(b => b.PatientId == patientId)
                .ToList());
        }

        public async Task<string?> GetLastBookingCodeAsync()
        {
            var lastBooking = await Task.Run(() => _context.Set<Booking>()
                .OrderByDescending(b => b.CreateDate)
                .FirstOrDefault());
            return lastBooking?.BookingCode;
        }

    }
}
