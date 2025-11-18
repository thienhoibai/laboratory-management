
using Azure;
using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
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

        public async Task<IEnumerable<BookingResponseDTO>> GetAllBookingsByDateAsync
            (DateOnly date, string? keyword, string? sortBy, string? sortDirection, int pageSize, int pageNumber)
        {
            var appointmentSlots = await _appointmentSlotService.GetAppointmentSlotsByDateAsync(date, 1, int.MaxValue);
            IEnumerable<Infrastructure.Models.Booking> bookings = new List<Infrastructure.Models.Booking>();
            
            foreach (var slot in appointmentSlots)
            {
                var slotBookings = await _bookingRepository.GetBookingsByAppointmentSlotSearchableAsync
                    (slot.SlotId, keyword, sortBy, sortDirection);
                bookings = bookings.Concat(slotBookings!);
            }

            bookings = bookings.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToList();


            var bookingResponses = new List<BookingResponseDTO>();
            foreach (var booking in bookings)
            {
                bookingResponses.Add(new BookingResponseDTO
                {
                    BookingCode = booking.BookingCode ??= "",
                    BookingId = booking.BookingId,
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

        public async Task<ResponseMessage> CreateBookingAsync (BookingRequestDTO bookingRequest)
        {
            ResponseMessage response = new ResponseMessage();
            if (!_appointmentSlotService.IsAppointmentsDateValid(bookingRequest.slotDTO.AppointmentDate))
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "Date must be 1 day in the future";
                return response;
            }



            if (!_appointmentSlotService.IsAppointmentSlotExists(
                    bookingRequest.slotDTO.AppointmentDate,
                    bookingRequest.slotDTO.TimeBlock))
            {
                await _appointmentSlotService.AddAppointmentSlotAsync(bookingRequest.slotDTO);

            }
            if (_appointmentSlotService.IsAppointmentSlotMaxedOut(bookingRequest.slotDTO))
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "This slot is currently full";
                return response;
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

            response.ResponseCode = ResponseCode.Success;
            response.Message = "Booking Successfully";
            response.InstancesCode = newBooking.BookingId;

            return response;

        } 

        #endregion

        public async Task<ResponseMessage> CheckInBooking (Guid bookingId)
        {
            var booking =  await _bookingRepository.GetByIdAsync(bookingId);
            var timeSlot = await _appointmentSlotService.GetAppointmentSlotByIdAsync((Guid)booking.AppointmentSlotId);
            TimeOnly lowerLimit = timeSlot.TimeBlock.Add(-TimeSpan.FromMinutes(30));
            TimeOnly upperLimit = timeSlot.TimeBlock.Add(TimeSpan.FromMinutes(30));
            ResponseMessage response = new ResponseMessage();
            if (booking == null)
            {
                response.ResponseCode = ResponseCode.NotFound;
                response.Message = "Booking not found";
                response.InstancesCode = bookingId;
                return response;
            }
                
            if (booking.Status != (byte?)BookingStatusEnum.Confirmed)
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "Booking is not in a state that allows check-in";
                response.InstancesCode = bookingId;
                return response;
            }
            if (DateOnly.FromDateTime(DateTime.Now) == timeSlot.AppointmentDate)
            {
                if ( TimeOnly.FromDateTime(DateTime.Now) >= lowerLimit && 
                    TimeOnly.FromDateTime(DateTime.Now) <= upperLimit ) 
                {
                    booking.Status = (byte?)BookingStatusEnum.InProgress;
                    booking.RunDate = DateOnly.FromDateTime(DateTime.Now);
                    await _bookingRepository.UpdateAsync(booking);

                    response.ResponseCode = ResponseCode.Success;
                    response.Message = "Check-in successful";
                    response.InstancesCode = bookingId;
                }
            }
            else 
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "Check-in is only allowed on the appointment date";
                response.InstancesCode = bookingId;
                return response;
            }
            return response;
            
        }

        public async Task<ResponseMessage> CheckOutBooking(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            ResponseMessage response = new ResponseMessage();
            if (booking == null)
            {
                response.ResponseCode = ResponseCode.NotFound;
                response.Message = "Booking not found";
                response.InstancesCode = bookingId;
                return response;
            }

            if (booking.Status != (byte?)BookingStatusEnum.InProgress)
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "Booking is not in a state that allows check-out";
                response.InstancesCode = bookingId;
                return response;
            }
            booking.Status = (byte?)BookingStatusEnum.Completed;
            await _bookingRepository.UpdateAsync(booking);
            response.ResponseCode = ResponseCode.Success;
            response.Message = "Check-out successful";
            response.InstancesCode = bookingId;
            return response;
        }

        internal async Task PaymentConfirmBooking (Guid bookingId)
        {
            ResponseMessage response = new ResponseMessage();

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }

            booking.Status = (byte?)BookingStatusEnum.Confirmed;
            await _bookingRepository.UpdateAsync(booking);
        }

    }
}
