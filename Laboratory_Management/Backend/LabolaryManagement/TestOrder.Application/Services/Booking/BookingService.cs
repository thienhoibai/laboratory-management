using Contracts.Notifications;
using Messaging.Notifications;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.DTOs.Bookings;
using TestOrder.Infrastructure.Enums;
using TestOrder.Infrastructure.Repository;
using TimeZoneConverter;

namespace TestOrder.Application.Services.Booking
{
    public class BookingService
    {
        private readonly BookingTestService _bookingTestService;
        private readonly AppointmentSlotService _appointmentSlotService;
        private readonly BookingRepository _bookingRepository;
        private readonly CatalogBundleService _catalogBundleService;
        private readonly TestBundleService _testBundleService;
        private readonly TestCatalogService _testCatalogService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IConfiguration configuration;

        private TimeZoneInfo timeZoneById;
        public BookingService(BookingRepository bookingRepository,
                              BookingTestService bookingTestService,
                              AppointmentSlotService appointmentSlotService,
                              CatalogBundleService catalogBundleService,
                              TestBundleService testBundleService,
                              TestCatalogService testCatalogService,
                              INotificationPublisher notificationPublisher,
                              IConfiguration configuration)
        {
            _bookingTestService = bookingTestService;
            _bookingRepository = bookingRepository;
            _appointmentSlotService = appointmentSlotService;
            _catalogBundleService = catalogBundleService;
            _testBundleService = testBundleService;
            _testCatalogService = testCatalogService;
            _notificationPublisher = notificationPublisher;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            timeZoneById = TZConvert.GetTimeZoneInfo(configuration.GetValue<string>("TimeZoneId") ?? "SE Asia Standard Time");
        }

        

        internal async Task<BookingResponseDTO> MapToDTOAsync(Infrastructure.Models.Booking booking)
        {
            var response = new BookingResponseDTO
            {
                BookingCode = booking.BookingCode ??= "",
                BookingId = booking.BookingId,
                PatientId = (Guid)booking.PatientId,
                PatientName = booking.PatientName ?? string.Empty,
                PatientPhoneNumber = booking.PatientPhone,
                PatientEmail = booking.PatientEmail,
                CreatedBy = booking.CreatedBy,
                BundleId = booking.BundleId,
                TotalAmount = booking.TotalPrice,
                CreatedAt = booking.CreateAt.HasValue
                        ? booking.CreateAt.Value
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
            };
            return response;

        }


        public async Task<object> GetAllBookingsAsync
            (DateOnly? date, string? keyword, string? sortBy, string? sortDirection, int pageSize, int pageNumber)
        {
            List<Infrastructure.Models.Booking> bookings = new List<Infrastructure.Models.Booking>();
            if (date != null)
            {
                var appointmentSlots = await _appointmentSlotService.GetAppointmentSlotsByDateAsync((DateOnly)date, 1, byte.MaxValue);

                if (appointmentSlots == null)
                {
                    throw new Exception("No Appointment Slots found on this date");
                }

                foreach (var slot in appointmentSlots)
                {
                    var slotBookings = await _bookingRepository.GetBookingsByAppointmentSlotSearchableAsync
                        (slot.SlotId, keyword);

                    if (slotBookings != null)
                    {
                        bookings.AddRange(slotBookings);
                    }
                }

            }
            else
            {
                bookings = await _bookingRepository.GetAllBookingsSearchableAsync(keyword);
            }

            if (bookings == null)
            {
                throw new Exception("No Bookings found");
            }

                var (pagedItems, totalItem) = await _bookingRepository.SortingAndPaging
                    (sortBy, sortDirection, pageSize, pageNumber, bookings);


            var totalPages = (int)Math.Ceiling((double)totalItem / pageSize);

            if (pagedItems == null)
            {
                throw new Exception("No Bookings found");
            }

            var bookingResponses = new List<BookingResponseDTO>();
            foreach (var booking in pagedItems)
            {
                bookingResponses.Add(MapToDTOAsync(booking).Result);
            }

            return new
            {
                totalItem,
                pageNumber,
                pageSize,
                totalPages,
                bookingResponses
            };
        }

        public async Task<BookingResponseDTO> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            var slotInfo = await _appointmentSlotService.GetAppointmentSlotInfo((Guid)booking.AppointmentSlotId);
            if (slotInfo == null)
                slotInfo = null;


            if (booking == null)
                throw new Exception("Booking not found");

            return await MapToDTOAsync(booking);
        }

        public async Task<object> GetBookingsByPatientIdAsync(Guid patientId, int pageNumber, int pageSize, byte? filterStatus)
        {


            var (bookings, totalItem) = await _bookingRepository.GetBookingsByPatientIdAsync(patientId, pageNumber, pageSize, filterStatus);

            var totalPages = (int)Math.Ceiling((double)totalItem / pageSize);

            var bookingResponses = new List<BookingResponseDTO>();

            if (bookings != null)
            {
                foreach (var booking in bookings)
                {
                    bookingResponses.Add(MapToDTOAsync(booking).Result);
                }
            }

            return new
            {
                totalItem,
                pageNumber,
                pageSize,
                totalPages,
                bookingResponses
            };
        }



        #region Create New Booking

        public async Task<ResponseMessage> CreateBookingAsync(BookingRequestDTO bookingRequest)
        {
            double totalPrice = 0d;
            int? bundleId = bookingRequest.BundleId.Value != 0 ? bookingRequest.BundleId.Value : null;

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

            if (bundleId != null)
            {
                totalPrice += _testBundleService.GetBundlePriceByIdAsync((int)bundleId);
            }

            if (bookingRequest.Catalogs != null && bookingRequest.Catalogs.Count > 0)
            {
                totalPrice += _testCatalogService.GetPriceForMultipleTests(bookingRequest.Catalogs);
            }



            var newBooking = new Infrastructure.Models.Booking
                {
                    BookingId = Guid.NewGuid(),
                    PatientId = bookingRequest.PatientId,
                    PatientName = bookingRequest.PatientName,
                    PatientPhone = bookingRequest.PatientPhoneNumber,
                    PatientEmail = bookingRequest.PatientEmail,
                    CreatedBy = bookingRequest.CreatedBy,
                    CreateAt = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneById),
                    BundleId = bundleId,
                    AppointmentSlotId = appointmentSlot.SlotId,
                    Status = (byte?)BookingStatusEnum.Pending,
                    BookingCode = nextCode,
                    TotalPrice = totalPrice
                };

            await _bookingRepository.AddAsync(newBooking);
            if (bookingRequest.Catalogs != null)
            {
                foreach (var catalogId in bookingRequest.Catalogs)
                {
                    _bookingTestService.AddBookingTestAsync(newBooking.BookingId, catalogId).Wait();
                }
            }
            if (bookingRequest.BundleId.HasValue)
            {
                var bundleCatalogs = await _catalogBundleService.GetCatalogidsByBundleIdAsync(bookingRequest.BundleId.Value);
                if (bundleCatalogs != null)
                {
                    foreach (var catalog in bundleCatalogs)
                    {
                        await _bookingTestService.AddBookingTestAsync(newBooking.BookingId, catalog);
                    }
                }
            }

            response.ResponseCode = ResponseCode.Success;
            response.Message = "Booking Successfully";
            response.InstancesCode = newBooking.BookingId;

            return response;

        }

        #endregion

        public async Task<ResponseMessage> CheckInBooking(Guid bookingId)
        {
            DateTime today = DateTime.Now;

            var booking =  await _bookingRepository.GetByIdAsync(bookingId);
            var timeSlot = await _appointmentSlotService.GetAppointmentSlotByIdAsync((Guid)booking.AppointmentSlotId);
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
            if (timeSlot.AppointmentDate != DateOnly.FromDateTime(today))
            {
                response.ResponseCode = ResponseCode.BadInstanceState;
                response.Message = "Check-in is only allowed on the appointment date";
                response.InstancesCode = bookingId;
                return response;
            }


            booking.Status = (byte)BookingStatusEnum.InProgress;
            booking.RunDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneById));
            await _bookingRepository.UpdateAsync(booking);
            response.ResponseCode = ResponseCode.Success;
            response.Message = "Check-in successful";
            response.InstancesCode = bookingId;
                
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

        internal async Task PaymentConfirmBooking(Guid bookingId, double? actualPaidAmount = null)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found");
            }
            if (booking.Status == (byte)BookingStatusEnum.Pending)
            {
                booking.Status = (byte?)BookingStatusEnum.Confirmed;
                if (actualPaidAmount.HasValue && actualPaidAmount.Value > 0)
                {
                    booking.TotalPrice = actualPaidAmount.Value;
                }
                await _bookingRepository.UpdateAsync(booking);
            }
            else
            {
                throw new Exception("Booking is not in a state that allows payment confirmation");
            }

            // ✅ GỬI EMAIL XÁC NHẬN BOOKING
            if (!string.IsNullOrWhiteSpace(booking.PatientEmail))
            {
                try
                {
                    var slot = await _appointmentSlotService.GetAppointmentSlotByIdAsync((Guid)booking.AppointmentSlotId!);

                    // Lấy thông tin bundle/test package
                    string testPackage = "Xét nghiệm tổng quát";
                    string totalAmount = "Đang cập nhật";

                    if (booking.BundleId.HasValue)
                    {
                        var bundle = await _testBundleService.GetByIdAsync(booking.BundleId.Value);
                        if (bundle != null)
                        {
                            testPackage = bundle.BundleName ?? "Xét nghiệm tổng quát";
                            if (bundle.Price.HasValue)
                            {
                                totalAmount = $"{bundle.Price.Value:N0}đ";
                            }
                        }
                    }
                    else if (booking.TotalPrice.HasValue)
                    {
                        totalAmount = $"{booking.TotalPrice.Value:N0}đ";
                    }

                    var templateData = new Dictionary<string, string>
                    {
                        { "BookingCode", booking.BookingCode ?? "N/A" },
                        { "TotalAmount", totalAmount },
                        { "TestPackage", testPackage },
                        { "Location", "Trung Tâm Xét nghiệm FPT - Tòa nhà F-Town 1, Lô T2, Đường D1, Khu Công nghệ Cao Sài Gòn, Phường Tân Phú, Thành phố Thủ Đức, Thành phố Hồ Chí Minh, Việt Nam.\r\n" },
                        { "PatientName", booking.PatientName ?? "Khách hàng" },
                        { "PatientEmail", booking.PatientEmail },
                        { "PatientPhone", booking.PatientPhone ?? "N/A" },
                        { "AppointmentDate", slot?.AppointmentDate.ToString("'Thứ' d, dd/MM/yyyy") ?? "Chưa xác định" },
                        { "AppointmentTime", slot?.TimeBlock.ToString(@"hh\:mm") ?? "Chưa xác định" }
                    };

                    await _notificationPublisher.PublishAsync("BookingConfirmation", new NotificationRequestedV1(
                        MessageId: Guid.NewGuid().ToString(),
                        Channel: "email",
                        To: booking.PatientEmail,
                        Template: "BookingConfirmation",
                        Data: templateData
                    ));

                    Console.WriteLine($"✅ Đã gửi yêu cầu email xác nhận booking #{booking.BookingCode} tới {booking.PatientEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Lỗi khi gửi email xác nhận booking: {ex.Message}");
                    // Không throw exception để không ảnh hưởng đến luồng thanh toán
                }
            }
        }
    }
}
