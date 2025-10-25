using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Domain
{
    public enum BookingStatus
    {
        Pending, //1
        Confirmed, //2
        InProgress, //3
        Completed, //4
        Cancelled //5
    }
    public class Booking
    {
        public long BookingId { get; }
        public long PatientId { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public DateTime? RunDate { get; set; }

        public long? RunBy { get; set; }

        public int BundleId { get; set; }

        public long AppointmentTimeSlotId { get; set; }



        public Booking() { }

        public Booking(long patientId, BookingStatus status, DateTime createdDate, long createdBy, int bundleId, long appointmentTimeSlotId)
        {
            PatientId = patientId;
            Status = status;
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            BundleId = bundleId;
            AppointmentTimeSlotId = appointmentTimeSlotId;
        }

        public bool isCancellable()
        {
            return Status == BookingStatus.Pending || Status == BookingStatus.Confirmed && Status != BookingStatus.Completed;
        }
    }
}
