using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.Bookings;
using TestOrder.Infrastructure.Repository;

namespace TestOrder.Application.Services.Booking
{
    public class BookingService
    {
        private readonly BookingTestService _bookingTestService;
        private readonly AppointmentSlotService _appointmentSlotService;
        private readonly BookingRepository _bookingRepository;

        public BookingService(BookingRepository bookingRepository)
        {
            _bookingTestService = new BookingTestService();
            _bookingRepository = bookingRepository;
            _appointmentSlotService = new AppointmentSlotService();
        }

        public async Task<Infrastructure.Models.Booking> CreateNewBooking(BookingRequestDTO bookingRequest)
        {
            if (!_appointmentSlotService.IsAppointmentsDateValid(bookingRequest.slotDTO.AppointmentDate))
            {
                throw new Exception("Invalid Booking Date");
            }

            if (!_appointmentSlotService.IsAppointmentSlotExists(bookingRequest.slotDTO.AppointmentDate,
                                                                 bookingRequest.slotDTO.TimeBlock))
            {
                _appointmentSlotService.AddAppointmentSlotAsync(bookingRequest.slotDTO).Wait();
            }

            var appointmentSlot = await _appointmentSlotService.GetAppointmentSlotByDateAndTimeAsync(
                                        bookingRequest.slotDTO.AppointmentDate,
                                        bookingRequest.slotDTO.TimeBlock);

            var newBooking = new Infrastructure.Models.Booking
                {
                BookingId = new Guid(),
                PatientId = bookingRequest.PatientId,
                PatientName = bookingRequest.PatientName,
                PatientPhone = bookingRequest.PatientPhoneNumber,
                CreatedBy = bookingRequest.CreatedBy,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                BundleId = bookingRequest.BundleId,
                AppointmentSlotId = appointmentSlot.SlotId,
                Status = (byte?)BookingStatusEnum.Pending
            };

             await _bookingRepository.AddAsync(newBooking);

            foreach (var catalogId in bookingRequest.Catalogs)
            {
                _bookingTestService.AddBookingTestAsync(newBooking.BookingId, catalogId).Wait();
            }


                return newBooking;

        }



    }
}
