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
            (Guid appointmentSlotId,string? keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                keyword = string.Empty;
            }
            var query =  _context.Set<Booking>()
                .Where(b => b.AppointmentSlotId == appointmentSlotId &&
            (b.BookingCode.Contains(keyword) ||
            b.PatientName.Contains(keyword) ||
            b.PatientEmail.Contains(keyword) ||
            b.PatientPhone.Contains(keyword))); 

            if (query == null || !query.Any())
            {
                return null;
            }


            return await query.ToListAsync();

        }

        public Task<IEnumerable<Booking>?> SortingAndPaging
            (string? sortBy, string? sortDirection, int pageSize, int pageNumber, IEnumerable<Booking> bookingList)
        {
            bool isDescending = sortDirection?.ToLower() == "desc";

            IQueryable<Booking> query = bookingList.AsQueryable();
            query = sortBy?.ToLower() switch
            {
                "bookingcode" => isDescending ? query.OrderByDescending(b => b.BookingCode) : query.OrderBy(b => b.BookingCode),
                "patientname" => isDescending ? query.OrderByDescending(b => b.PatientName) : query.OrderBy(b => b.PatientName),
                "patientemail" => isDescending ? query.OrderByDescending(b => b.PatientEmail) : query.OrderBy(b => b.PatientEmail),
                "patientphone" => isDescending ? query.OrderByDescending(b => b.PatientPhone) : query.OrderBy(b => b.PatientPhone),
                _ => query.OrderBy(b => b.BookingCode),
            };



            return Task.FromResult<IEnumerable<Booking>?>(query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList());

        }
       
    }
}
