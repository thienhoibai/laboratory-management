using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Bookings
{
    public class BookingRequestDTO
    {
        [Required]
        public long PatientId { get; set; }

        [Phone]
        [MaxLength(12)]
        public string? PatientPhoneNumber { get; set; }

        [Required]
        public string PatientName { get; set; } = string.Empty;

        public string? CreatedBy { get; set; }

        public int? BundleId { get; set; }

        public List<int> Catalogs { get; set; } = new List<int>();

        [Required]
        public DateOnly BookingDate { get; set; }

        [Required]
        public TimeOnly BookingTime { get; set; }

    }
}
