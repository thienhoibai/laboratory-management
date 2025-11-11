 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Application.DTOs.AppointmentSlots;

namespace TestOrder.Application.DTOs.Bookings
{
    public class BookingResponseDTO
    {
        [Required]
        public string BookingCode { get; set; } = string.Empty;
        [Required]
        public Guid PatientId { get; set; }

        [Phone]
        [MaxLength(12)]
        [MinLength(10)]
        public string? PatientPhoneNumber { get; set; }

        [Required]

        public string PatientName { get; set; } = string.Empty;
        public string? PatientEmail { get; set; }

        public string? CreatedBy { get; set; }
        public int? BundleId { get; set; }
        public DateTime CreatedDate { get; set; }

        public DateTime? RunDate { get; set; }
        public string RanBy { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public AppointmentSlotDTO slotInfo { get; set; } = new AppointmentSlotDTO();


    }
}
