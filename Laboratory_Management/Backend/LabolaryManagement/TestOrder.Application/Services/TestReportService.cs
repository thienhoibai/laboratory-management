// OpenXML
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TestOrder.Application.Services.Booking;
using TestOrder.Infrastructure.Models;
using TestOrder.Infrastructure.Repository;
using Xceed.Document.NET;
using Xceed.Words.NET;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Font = Xceed.Document.NET.Font;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace TestOrder.Application.Services
{
    public class TestReportService
    {
        private readonly TestReportRepository _testReportRepository;
        private readonly TestResultRepository _testResultRepository;
        private readonly BookingService _bookingService;
        private readonly IHttpClientFactory _httpClientFactory;

        public TestReportService(
            TestReportRepository testReportRepository,
            TestResultRepository testResultRepository,
            BookingService bookingService,
            IHttpClientFactory httpClientFactory)
        {
            _testReportRepository = testReportRepository;
            _testResultRepository = testResultRepository;
            _bookingService = bookingService;
            _httpClientFactory = httpClientFactory;
        }



        internal async Task<byte[]> GenerateReport(Guid bookingId)
        {
            var testResult = await _testResultRepository.GetResultByBookingId(bookingId);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (testResult == null)
                throw new ArgumentException("No test result found for the specified booking ID.");

            PatientDetailDto? patient = null;
            if (booking.PatientId != Guid.Empty)
                patient = await GetPatientByIdAsync(booking.PatientId);

            // Tạo file Word
            var doc = DocX.Create($"TestReport_{testResult.BookingCode}.docx");
            doc.SetDefaultFont(new Font("Calibri"));

            doc.MarginLeft = 50;
            doc.MarginRight = 50;
            doc.MarginTop = 50;
            doc.MarginBottom = 50;

            // ================================
            // TIÊU ĐỀ
            // ================================
            var title = doc.InsertParagraph("KẾT QUẢ XÉT NGHIỆM")
                           .Bold()
                           .FontSize(24)
                           .Color(Xceed.Drawing.Color.Black);
            title.Alignment = Alignment.center;
            title.SpacingAfter(10);

            var codePara = doc.InsertParagraph($"Mã Đặt Lịch: {testResult.BookingCode}")
                              .Bold()
                              .FontSize(14);
            codePara.Alignment = Alignment.center;
            codePara.SpacingAfter(20);

            // ================================
            // THÔNG TIN BỆNH NHÂN
            // ================================
            doc.InsertParagraph("I. THÔNG TIN BỆNH NHÂN")
                .Bold().FontSize(16)
                .Color(Xceed.Drawing.Color.DarkBlue)
                .SpacingAfter(10);

            var info = doc.AddTable(6, 2);
            info.Design = TableDesign.TableGrid;
            info.Alignment = Alignment.left;
            info.SetWidthsPercentage(new float[] { 25f, 75f });

            void Row(int row, string label, string value)
            {
                info.Rows[row].Cells[0].Paragraphs[0].Append(label).Bold();
                info.Rows[row].Cells[1].Paragraphs[0].Append(value ?? "N/A");
            }

            Row(0, "Họ Tên", patient?.FullName ?? booking.PatientName);
            Row(1, "Giới Tính", GetGenderDisplay(patient?.Gender ?? 0));
            Row(2, "Ngày Sinh", patient?.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A");
            Row(3, "Địa Chỉ", patient?.Address ?? "N/A");
            Row(4, "Nhóm Máu", GetBloodTypeDisplay(patient?.BloodType ?? BloodType.Unknown));
            Row(5, "SĐT", patient?.Phone ?? booking.PatientPhoneNumber ?? "N/A");

            doc.InsertTable(info);
            doc.InsertParagraph().SpacingAfter(20);

            // ================================
            // KẾT QUẢ XÉT NGHIỆM
            // ================================
            doc.InsertParagraph("II. KẾT QUẢ XÉT NGHIỆM")
                .Bold().FontSize(16)
                .Color(Xceed.Drawing.Color.DarkBlue)
                .SpacingAfter(10);

            foreach (var catalog in testResult.Catalogs)
            {
                doc.InsertParagraph($"• {catalog.CatalogName}")
                    .Bold().FontSize(14)
                    .Color(Xceed.Drawing.Color.DarkBlue)
                    .SpacingBefore(8)
                    .SpacingAfter(5);

                var table = doc.AddTable(catalog.Parameters.Count + 1, 4);
                table.Design = TableDesign.ColorfulListAccent1;

                // Header
                table.Rows[0].Cells[0].Paragraphs[0].Append("Tên Tham Số").Bold();
                table.Rows[0].Cells[1].Paragraphs[0].Append("Kết Quả").Bold();
                table.Rows[0].Cells[2].Paragraphs[0].Append("Đơn Vị").Bold();
                table.Rows[0].Cells[3].Paragraphs[0].Append("Giá Trị Tham Chiếu").Bold();

                for (int i = 0; i < catalog.Parameters.Count; i++)
                {
                    var p = catalog.Parameters[i];

                    table.Rows[i + 1].Cells[0].Paragraphs[0].Append(p.Name);

                    var valCell = table.Rows[i + 1].Cells[1].Paragraphs[0].Append(p.Value);

                    // Highlight bất thường
                    if (double.TryParse(p.Value, out double val) &&
                        p.ReferenceRange?.Contains("-") == true)
                    {
                        try
                        {
                            var range = p.ReferenceRange.Split('-');
                            double min = double.Parse(range[0]);
                            double max = double.Parse(range[1]);

                            if (val < min || val > max)
                            {
                                valCell.Color(Xceed.Drawing.Color.Red).Bold();
                            }
                        }
                        catch
                        {
                            // ignore parse errors
                        }
                    }

                    table.Rows[i + 1].Cells[2].Paragraphs[0].Append(p.Unit);
                    table.Rows[i + 1].Cells[3].Paragraphs[0].Append(p.ReferenceRange);
                }

                doc.InsertTable(table);
                doc.InsertParagraph().SpacingAfter(10);
            }

            // ================================
            // “FOOTER” DẠNG PARAGRAPH (KHÔNG DÙNG AddFooter)
            // ================================
            var datePara = doc.InsertParagraph($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}")
                              .Italic()
                              .FontSize(10);
            datePara.Alignment = Alignment.right;

            var pagePara = doc.InsertParagraph("Trang ")
                              .FontSize(10);
            pagePara.AppendPageNumber(PageNumberFormat.normal);
            pagePara.Append(" / ");
            pagePara.AppendPageCount(PageNumberFormat.normal);
            pagePara.Alignment = Alignment.center;

            // ================================
            // RETURN
            // ================================
            using var ms = new MemoryStream();
            doc.SaveAs(ms);
            return ms.ToArray();
        }



        // ======== API CALL & helper =========

        private async Task<PatientDetailDto?> GetPatientByIdAsync(Guid patientId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PatientApi");
                var response = await client.GetAsync($"v1/patients/internal/{patientId}");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<PatientDetailDto>();

                return null;
            }
            catch
            {
                return null;
            }
        }

        private string GetGenderDisplay(byte gender)
        {
            return gender switch
            {
                0 => "Không xác định",
                1 => "Nam",
                2 => "Nữ",
                3 => "Khác",
                _ => "N/A"
            };
        }

        private string GetBloodTypeDisplay(BloodType bloodType)
        {
            return bloodType switch
            {
                BloodType.Unknown => "Chưa xác định",
                BloodType.APositive => "A+",
                BloodType.ANegative => "A-",
                BloodType.BPositive => "B+",
                BloodType.BNegative => "B-",
                BloodType.ABPositive => "AB+",
                BloodType.ABNegative => "AB-",
                BloodType.OPositive => "O+",
                BloodType.ONegative => "O-",
                _ => "N/A"
            };
        }

        public async Task<bool> CreateNewReport(Guid bookingId)
        {
            var resultData = await GenerateReport(bookingId);

            var report = new TestReport
            {
                BookingId = bookingId,
                ResultData = resultData,
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

    // DTO giống của anh
    public class PatientDetailDto
    {
        public Guid PatientId { get; set; }
        public string? FullName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public byte Gender { get; set; }
        public BloodType BloodType { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CitizenId { get; set; }
        public string? InsuranceNumber { get; set; }
    }

    public enum BloodType : byte
    {
        Unknown = 0,
        APositive = 1,
        ANegative = 2,
        BPositive = 3,
        BNegative = 4,
        ABPositive = 5,
        ABNegative = 6,
        OPositive = 7,
        ONegative = 8
    }
}
