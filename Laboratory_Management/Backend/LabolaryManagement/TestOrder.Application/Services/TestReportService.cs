using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOrder.Application.Services.Booking;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;
using Xceed.Words.NET;

namespace TestOrder.Application.Services
{
    public class TestReportService
    {
        private readonly TestReportRepository _testReportRepository;
        private readonly TestResultRepository testResultRepository;
        private readonly BookingService _bookingService;

        public TestReportService(TestReportRepository testReportRepository, 
                                 TestResultRepository testResultRepository,
                                 BookingService bookingService)
        {
            _testReportRepository = testReportRepository;
            this.testResultRepository = testResultRepository;
            _bookingService = bookingService;
        }

        internal async Task<byte[]> GenerateReport (Guid bookingId)
        {
            var testResult = await testResultRepository.GetResultByBookingId(bookingId);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (testResult == null)
            {
                throw new ArgumentException("No test result found for the specified booking ID.");
            }

            var doc = DocX.Create($"TestReport_{testResult.BookingCode}.docx");
            

            doc.InsertParagraph($"Kết Quả Xét Nghiệm")
                .FontSize(20)
                .Bold()
                .Alignment = Xceed.Document.NET.Alignment.center;

            doc.InsertParagraph($"Mã Đặt Lịch: {testResult.BookingCode}")
                .FontSize(14)
                .SpacingAfter(20);

            doc.InsertParagraph($"Họ Tên: {booking.PatientName}")
                .FontSize(14);
            doc.InsertParagraph($"SĐT: {booking.PatientPhoneNumber}");

            foreach (var catalog in testResult.Catalogs)
            {
                doc.InsertParagraph($"\nTên Gói Xét Nghiệm: {catalog.CatalogName}")
                    .FontSize(16)
                    .Bold()
                    .SpacingBefore(10)
                    .SpacingAfter(10);
                var table = doc.AddTable(catalog.Parameters.Count + 1, 4);
                table.Design = Xceed.Document.NET.TableDesign.ColorfulList;
                // Header row
                table.Rows[0].Cells[0].Paragraphs[0].Append("Tên Tham Số").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Kết Quả").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Đơn Vị").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Giá Trị Tham Chiếu").Bold();
                // Data rows
                for (int i = 0; i < catalog.Parameters.Count; i++)
                {
                    var parameter = catalog.Parameters[i];
                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append(parameter.Name);
                    table.Rows[i + 1].Cells[1].Paragraphs[0].Append(parameter.Value);
                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(parameter.Unit);
                    table.Rows[i + 1].Cells[3].Paragraphs[0].Append(parameter.ReferenceRange);
                }
                doc.InsertTable(table);
            }
            
            using (var ms = new MemoryStream())
            {
                doc.SaveAs(ms);
                return ms.ToArray();
            }
        }

        public async Task<bool> CreateNewReport(Guid bookingId)
        {
            var resultData = GenerateReport(bookingId);
            if (resultData == null)
            {
                throw new ArgumentException("Unable to generate report data.");
            }
            var report = new TestReport
            {
                BookingId = bookingId,
                ResultData = await resultData,
                Filename = $"TestReport_{bookingId}.docx",
                FileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            };
            await _testReportRepository.AddAsync(report);
            return true;
        }   

        public async Task<TestReport?> GetReportByBookingId(Guid bookingId)
        {
            return await _testReportRepository.GetReportByBookingId(bookingId);
        }
    }
}
