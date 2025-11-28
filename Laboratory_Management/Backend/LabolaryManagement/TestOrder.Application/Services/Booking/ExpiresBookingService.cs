using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Infrastructure.Repository;
using TimeZoneConverter;

namespace TestOrder.Application.Services.Booking
{
    public class ExpiresBookingService : BackgroundService
    {
        private int ExpiryDuration = 15;
        private int CheckInterval = 1;

        private readonly BookingRepository _bookingRepository;
        private readonly ILogger<ExpiresBookingService> _logger;
        private TimeZoneInfo timeZone;

        public ExpiresBookingService(BookingRepository bookingRepository, 
                                     ILogger<ExpiresBookingService> logger,
                                     IConfiguration configuration)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
            timeZone = TZConvert.GetTimeZoneInfo(configuration["TimeZoneId"] ?? "SE Asia Standard Time ");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Begin ExpiringBooking");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CancelBookingAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while expire booking");
                }
                try
                {
                   await Task.Delay(TimeSpan.FromMinutes(CheckInterval), stoppingToken);
                }
                catch (TaskCanceledException tEx)
                {
                    _logger.LogError(tEx, "Cancelled");
                }
            }

        }

        internal async Task CancelBookingAsync(CancellationToken stoppingToken)
        {
            var timeNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);

            var expiryThreshold = timeNow.AddMinutes(-ExpiryDuration);

            var bookings = await _bookingRepository.GetPendingBookingAsync(expiryThreshold);

            if (bookings != null)
            {
                foreach (var booking in bookings)
                {
                    booking.Status = (byte)Infrastructure.Enums.BookingStatusEnum.Cancelled;
                    booking.CancelAt = timeNow;
                    _logger.LogInformation($"Booking {booking.BookingCode} has been cancelled due to expiry.");
                    await _bookingRepository.UpdateAsync(booking);
                }
                _logger.LogInformation($"Total {bookings.Count()} bookings have been cancelled due to expiry at {expiryThreshold} .");
            }

        }

    }
}
