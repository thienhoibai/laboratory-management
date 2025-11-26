using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class Booking
{
    public Guid BookingId { get; set; }

    public Guid? PatientId { get; set; }

    public byte? Status { get; set; }

    public string? PatientName { get; set; }

    public string? PatientPhone { get; set; }

    public string? PatientEmail { get; set; }

    public string? BookingCode { get; set; }

    public Guid? AppointmentSlotId { get; set; }

    public DateOnly? CreateDate { get; set; }

    public TimeOnly? CreateTime { get; set; }

    public string? CreatedBy { get; set; }

    public DateOnly? RunDate { get; set; }

    public string? RanBy { get; set; }

    public int? BundleId { get; set; }

    public double? TotalPrice { get; set; }

    public virtual AppointmentSlot? AppointmentSlot { get; set; }

    public virtual ICollection<BookingTest> BookingTests { get; set; } = new List<BookingTest>();

    public virtual TestBundle? Bundle { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<PaymentEnvoice> PaymentEnvoices { get; set; } = new List<PaymentEnvoice>();

    public virtual ICollection<TestReport> TestReports { get; set; } = new List<TestReport>();
}
