using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestOrder.Domain
{
    public class AppointmentSlot
    {
        public long AppointmentTimeSlotId { get; }
        public DateOnly AppointmentDate { get; set; }

        public int TimeBlockId { get; set; }

        public int MaxBookings { get; set; }

        private readonly List<Booking> _bookings = new();

        public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();

        public AppointmentSlot() { }

        public AppointmentSlot(DateOnly appointmentDate, int timeBlockId, int maxBookings)
        {
            AppointmentDate = appointmentDate;
            TimeBlockId = timeBlockId;
            MaxBookings = maxBookings;
        }

        public bool IsFullyBooked()
        {
            return _bookings.Count >= MaxBookings;
        }

        public void AddBooking(Booking booking)
        {
            if (!IsFullyBooked())
            {
                throw new InvalidOperationException("Maximum bookings reached for this time slot.");
            }
            _bookings.Add(booking);
        }

        public bool IsEligibleBookingDate(DateOnly currentDate)
        {
            if (AppointmentDate <= currentDate)
            {
                throw new InvalidOperationException("Can only book appointment for the next day onward");
            }
            else
            {
                return true;
            }
        }

        public bool isCancellableDate()
        {
            DateOnly currentDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")));
            if (AppointmentDate <= currentDate)
            {
                throw new Exception("Appointment date has passed or is today, cannot cancel booking.");
            }
            else {
                return true;
            }
        }
    }
}
