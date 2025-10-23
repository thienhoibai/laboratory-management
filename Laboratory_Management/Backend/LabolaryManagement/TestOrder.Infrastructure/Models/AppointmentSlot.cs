using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class AppointmentSlot
{
    public long SlotId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public int TimeBlockId { get; set; }

    public int MaxBooking { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual TimeBlock TimeBlock { get; set; } = null!;
}
