using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Base;
using TestOrder.Infrastructure.Enums;
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

        public async Task<IEnumerable<Booking?>> GetPendingBookingAsync(DateTime expiryThreshold)
        {
            return await _context.Set<Booking>()
                .Where(b => b.Status == (byte)BookingStatusEnum.Pending && b.CreateAt <= expiryThreshold)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Booking>? items, int totalItems)> GetBookingsByPatientIdAsync
            (Guid patientId, int pageNumber, int pageSize, byte? filterStatus)
        {
            var query = _context.Set<Booking>()
                .Where(b => b.PatientId == patientId);
            if (query == null || !query.Any())
            {
                throw new ArgumentException("No bookings found for the specified patient ID.");
            }

            if (filterStatus.HasValue)
            {
                query = query.Where(b => b.Status == filterStatus.Value);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.BookingCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalItems);

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

        public Task<(IEnumerable<Booking>? items, int totalItems)> SortingAndPaging
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

            var totalItems = query.Count();

            IEnumerable<Booking>? items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<(IEnumerable<Booking>?, int)>((items, totalItems));
        }
       
    }
}
