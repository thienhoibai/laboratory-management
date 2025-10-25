using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOrder.Application.DTOs.Bookings
{
    public class BookingDTO
    {  
        [Required]
        long PatientId { get; set; }
        
        [Required]
        string? CreatedBy { get; set; }

        int? BundleId { get; set; } = null;

        [Required]
        string PatientName { get; set; }

        [Phone]
        string PatientPhone { get; set; }

    }
}
