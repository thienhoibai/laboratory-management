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

        public async Task<IEnumerable<BookingResponseDTO>> GetAllBookingsAsync(int pageNumber)
        {
            var bookings = await _bookingRepository.GetAllPagedAsync(pageNumber);
            var bookingResponses = new List<BookingResponseDTO>();
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
                        : "Unknown",
                    SlotInfo = await _appointmentSlotService.GetAppointmentSlotInfo((Guid)booking.AppointmentSlotId),
                    TestCatalogs = await _bookingTestService.GetCatalogIdsByBookingIdAsync(booking.BookingId)
                });
            }
            return bookingResponses;
        }

        public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            var slotInfo = await _appointmentSlotService.GetAppointmentSlotInfo((Guid)booking.AppointmentSlotId);
            if (slotInfo == null)
                slotInfo = null;


            if (booking == null)
                throw new Exception("Booking not found");

            return new BookingResponseDTO
            {
                BookingCode = booking.BookingCode ??="",
                PatientId = (Guid)booking.PatientId,
                BookingId = booking.BookingId,
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
                    : "Unknown",
                SlotInfo = slotInfo,
                TestCatalogs = await _bookingTestService.GetCatalogIdsByBookingIdAsync(booking.BookingId)
            };
        }

        public async Task<List<BookingResponseDTO>> GetBookingsByPatientIdAsync(Guid patientId, int pageNumber, int pageSize)
        {
            var bookings = await _bookingRepository.GetBookingsByPatientIdAsync(patientId, pageNumber, pageSize);
            var bookingResponses = new List<BookingResponseDTO>();

            if (bookings != null)
            {
                foreach (var booking in bookings)
                {
                    bookingResponses.Add(new BookingResponseDTO
                    {
                        BookingCode = booking.BookingCode ??= "",
                        PatientId = (Guid)booking.PatientId,
                        BookingId = booking.BookingId,
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
                            : "Unknown",
                        SlotInfo = await _appointmentSlotService.GetAppointmentSlotInfo((Guid)booking.AppointmentSlotId),
                        TestCatalogs = await _bookingTestService.GetCatalogIdsByBookingIdAsync(booking.BookingId)

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
                
            }
            if (_appointmentSlotService.IsAppointmentSlotMaxedOut(bookingRequest.slotDTO))
            {
                return -2;
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
                BundleId = bookingRequest.BundleId.Value != 0 ? bookingRequest.BundleId : null,
                AppointmentSlotId = appointmentSlot.SlotId,
                Status = (byte?)BookingStatusEnum.Pending,
                BookingCode = nextCode
            };

            await _bookingRepository.AddAsync(newBooking);
            if (bookingRequest.Catalogs != null)
            {
                foreach (var catalogId in bookingRequest.Catalogs)
                {
                    _bookingTestService.AddBookingTestAsync(newBooking.BookingId, catalogId).Wait();
                }
            }
            return 0;

        }
        #endregion

        public async Task<int> CheckInBooking (Guid bookingId)
        {
            var booking =  await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return -1;
            if (booking.Status != (byte?) BookingStatusEnum.Confirmed) return -2;
            booking.Status = (byte?)BookingStatusEnum.CheckedIn;
            booking.RunDate = DateOnly.FromDateTime(DateTime.Now);
            await _bookingRepository.UpdateAsync(booking);
            return 0;

        }

        public async Task<int> CheckOutBooking(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return -1;
            if (booking.Status != (byte?)BookingStatusEnum.InProgress) return -2;
            booking.Status = (byte?)BookingStatusEnum.Completed;
            await _bookingRepository.UpdateAsync(booking);
            return 0;
        }
    }
}
