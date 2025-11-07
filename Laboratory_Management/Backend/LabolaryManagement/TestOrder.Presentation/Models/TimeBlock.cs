using System;
using System.Collections.Generic;

namespace TestOrder.Presentation.Models;

public partial class TimeBlock
{
    public int TimeBlockId { get; set; }

    public TimeOnly TimeBlock1 { get; set; }

    public virtual ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();
}
