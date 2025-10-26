using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Base;

namespace TestOrder.Infrastructure.Repository
{
    public class AppointmentSlotRepository : GenericRepository<AppointmentSlot>
    {
        public AppointmentSlotRepository(Data.TestOrderDBContext context) : base(context)
        {
        }

        public AppointmentSlotRepository() : base()
        {
        }

        public Task<List<AppointmentSlot>> GetByDateAsync(DateOnly appointmentDate)
        {
            return Task.Run(() =>
            {
                return _context.Set<AppointmentSlot>()
                    .Where(a => a.AppointmentDate == appointmentDate)
                    .ToList();
            });
        }

        public async Task<AppointmentSlot?> GetByDateAndTimeAsync(DateOnly appointmentDate, int timeBlockId)
        {
            return await Task.Run(() =>
            {
                return _context.Set<AppointmentSlot>()
                    .FirstOrDefault(a => a.AppointmentDate == appointmentDate && a.TimeBlockId == timeBlockId);
            });
        }

        public async Task<int> GetBookingsCountForSlot(long appointmentSlotId)
        {
            return await Task.Run(() =>
            {
                return _context.Set<Booking>()
                    .Count(b => b.AppointmentSlotId == appointmentSlotId);
            });
        }
    }
}
