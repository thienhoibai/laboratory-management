using System;
using System.Collections.Generic;
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

        public BookingService(BookingRepository bookingRepository,
                              BookingTestService bookingTestService,
                              AppointmentSlotService appointmentSlotService)
        {
            _bookingTestService = bookingTestService;
            _bookingRepository = bookingRepository;
            _appointmentSlotService = appointmentSlotService;
        }

        public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            return new BookingResponseDTO
            {
                BookingCode = booking.BookingCode ??="",
                PatientId = (Guid)booking.PatientId,
                PatientName = booking.PatientName ?? string.Empty,
                PatientPhoneNumber = booking.PatientPhone,
                PatientEmail = booking.PatientEmail,
                CreatedBy = booking.CreatedBy,
                BundleId = booking.BundleId,
                CreatedDate = booking.CreateDate.HasValue
                    ? booking.CreateDate.Value.ToDateTime(new TimeOnly(0, 0))
                    : DateTime.MinValue,
                RunDate = booking.RunDate.HasValue
                    ? booking.RunDate.Value.ToDateTime(new TimeOnly(0, 0))
                    : (DateTime?)null,
                RanBy = booking.RanBy ?? string.Empty,
                Status = booking.Status.HasValue
                    ? ((BookingStatusEnum)booking.Status.Value).ToString()
                    : "Unknown"
            };
        }

        public async Task<List<BookingResponseDTO>> GetBookingsByPatientIdAsync(Guid patientId)
        {
            var bookings = await _bookingRepository.GetBookingsByPatientIdAsync(patientId);
            var bookingResponses = new List<BookingResponseDTO>();

            if (bookings != null)
            {
                foreach (var booking in bookings)
                {
                    bookingResponses.Add(new BookingResponseDTO
                    {
                        BookingCode = booking.BookingCode ??= "",
                        PatientId = (Guid)booking.PatientId,
                        PatientName = booking.PatientName ?? string.Empty,
                        PatientPhoneNumber = booking.PatientPhone,
                        PatientEmail = booking.PatientEmail,
                        CreatedBy = booking.CreatedBy,
                        BundleId = booking.BundleId,
                        CreatedDate = booking.CreateDate.HasValue
                            ? booking.CreateDate.Value.ToDateTime(new TimeOnly(0, 0))
                            : DateTime.MinValue,
                        RunDate = booking.RunDate.HasValue
                            ? booking.RunDate.Value.ToDateTime(new TimeOnly(0, 0))
                            : (DateTime?)null,
                        RanBy = booking.RanBy ?? string.Empty,
                        Status = booking.Status.HasValue
                            ? ((BookingStatusEnum)booking.Status.Value).ToString()
                            : "Unknown"
                    });
                }
            }

            return bookingResponses;
        }

        #region Create New Booking
        public async Task<int> CreateNewBooking(BookingRequestDTO bookingRequest)
        {
            if (!_appointmentSlotService.IsAppointmentsDateValid(bookingRequest.slotDTO.AppointmentDate))
                return -1;

            

            if (!_appointmentSlotService.IsAppointmentSlotExists(
                    bookingRequest.slotDTO.AppointmentDate,
                    bookingRequest.slotDTO.TimeBlock))
            {
                await _appointmentSlotService.AddAppointmentSlotAsync(bookingRequest.slotDTO);
                if (_appointmentSlotService.IsAppointmentSlotMaxedOut(bookingRequest.slotDTO))
                {
                    return -2;
                }
            }

            var appointmentSlot = await _appointmentSlotService.GetAppointmentSlotByDateAndTimeAsync(
                bookingRequest.slotDTO.AppointmentDate,
                bookingRequest.slotDTO.TimeBlock);

            var lastBooking = await _bookingRepository.GetLastBookingCodeAsync();
            int nextNumber = 1;
            if (lastBooking != null && !string.IsNullOrEmpty(lastBooking))
            {
                if (int.TryParse(lastBooking, out int lastNumber))
                    nextNumber = lastNumber + 1;
            }
            string nextCode = nextNumber.ToString("D6");
            var newBooking = new Infrastructure.Models.Booking
            {
                BookingId = Guid.NewGuid(),
                PatientId = bookingRequest.PatientId,
                PatientName = bookingRequest.PatientName,
                PatientPhone = bookingRequest.PatientPhoneNumber,
                PatientEmail = bookingRequest.PatientEmail,
                CreatedBy = bookingRequest.CreatedBy,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                BundleId = bookingRequest.BundleId.Value != 0 ? bookingRequest.BundleId : null ,
                AppointmentSlotId = appointmentSlot.SlotId,
                Status = (byte?)BookingStatusEnum.Pending,
                BookingCode = nextCode
            };

            await _bookingRepository.AddAsync(newBooking);
        if (bookingRequest.Catalogs != null) { 
            foreach (var catalogId in bookingRequest.Catalogs)
            {
                _bookingTestService.AddBookingTestAsync(newBooking.BookingId, catalogId).Wait();
            }
        }
             return 0;

        }
        #endregion
    }
}
