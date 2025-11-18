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

        public async Task<IEnumerable<Booking>?> GetBookingsByPatientIdAsync(Guid patientId, int pageNumber, int pageSize)
        {
            return await _context.Set<Booking>()
                .Where(b => b.PatientId == patientId)
                .Skip((pageNumber - 1)* pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<string?> GetLastBookingCodeAsync()
        {
            var lastBooking = await Task.Run(() => _context.Set<Booking>()
                .OrderByDescending(b => b.BookingCode)
                .FirstOrDefault());
            return lastBooking?.BookingCode;
        }

        public async Task<IEnumerable<Booking>?> GetBookingsByAppointmentSlotSearchableAsync 
            (Guid appointmentSlotId, int pageNumber, int pageSize, string? keyword, string? sortBy, string? sortDirection)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                keyword = string.Empty;
            }
            var query = _context.Set<Booking>()
                .Where(b => b.AppointmentSlotId == appointmentSlotId &&
                            b.BookingCode.Contains(keyword) || 
                            b.PatientName.Contains(keyword) ||
                            b.PatientEmail.Contains(keyword) ||
                            b.PatientPhone.Contains(keyword));

            bool desc = sortDirection?.ToLower() == "desc";

            query = (sortBy?.ToLower()) switch
            {
                "bookingcode" => desc ? query.OrderByDescending(b => b.BookingCode) : query.OrderBy(b => b.BookingCode),
                "patientname" => desc ? query.OrderByDescending(b => b.PatientName) : query.OrderBy(b => b.PatientName),
                "patientemail" => desc ? query.OrderByDescending(b => b.PatientEmail) : query.OrderBy(b => b.PatientEmail),
                "patientphone" => desc ? query.OrderByDescending(b => b.PatientPhone) : query.OrderBy(b => b.PatientPhone),
                "status" => desc ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                _ => query.OrderBy(b => b.BookingCode),
            };

            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return query;
        }
       
    }
}
