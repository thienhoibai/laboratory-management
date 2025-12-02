using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly IHttpClientFactory _httpClientFactory;

        // Địa chỉ phòng khám cố định
        private const string CLINIC_ADDRESS = "Trung Tâm Xét nghiệm FPT - Tòa nhà F-Town 1, Lô T2, Đường D1, Khu Công nghệ Cao Sài Gòn, Phường Tân Phú, Thành phố Thủ Đức, Thành phố Hồ Chí Minh, Việt Nam.";

        public TestReportService(TestReportRepository testReportRepository, 
                                 TestResultRepository testResultRepository,
                                 BookingService bookingService,
                                 IHttpClientFactory httpClientFactory)
        {
            _testReportRepository = testReportRepository;
            this.testResultRepository = testResultRepository;
            _bookingService = bookingService;
            _httpClientFactory = httpClientFactory;
        }

        internal async Task<byte[]> GenerateReport (Guid bookingId)
        {
            var testResult = await testResultRepository.GetResultByBookingId(bookingId);
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (testResult == null)
            {
                throw new ArgumentException("No test result found for the specified booking ID.");
            }

            // Lấy thông tin patient từ Patient API
            PatientDetailDto? patient = null;
            if (booking.PatientId != Guid.Empty)
            {
                patient = await GetPatientByIdAsync(booking.PatientId);
                if (patient != null)
                {
                    Console.WriteLine($"✅ Đã lấy thông tin patient: {patient.FullName}, Gender: {patient.Gender}, DOB: {patient.DateOfBirth}, BloodType: {patient.BloodType}");
                }
                else
                {
                    Console.WriteLine($"⚠️ Không lấy được thông tin patient từ API cho PatientId: {booking.PatientId}");
                }
            }

            var doc = DocX.Create($"TestReport_{testResult.BookingCode}.docx");
            
            // Tiêu đề chính
            doc.InsertParagraph("KẾT QUẢ XÉT NGHIỆM")
                .FontSize(20)
                .Bold()
                .Alignment = Xceed.Document.NET.Alignment.center;

            // Địa chỉ phòng khám
            doc.InsertParagraph(CLINIC_ADDRESS)
                .FontSize(11)
                .Italic()
                .Alignment = Xceed.Document.NET.Alignment.center;
            
            doc.InsertParagraph("─────────────────────────────────────")
                .FontSize(12)
                .Alignment = Xceed.Document.NET.Alignment.center;

            // Mã đặt lịch
            doc.InsertParagraph($"\nMã Đặt Lịch: {testResult.BookingCode}")
                .FontSize(14)
                .Bold()
                .SpacingAfter(20);

            // THÔNG TIN BỆNH NHÂN
            doc.InsertParagraph("THÔNG TIN BỆNH NHÂN")
                .FontSize(16)
                .Bold()
                .SpacingBefore(10)
                .SpacingAfter(10);

            if (patient != null)
            {
                doc.InsertParagraph($"Họ Tên: {patient.FullName ?? booking.PatientName}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Giới Tính: {GetGenderDisplay(patient.Gender)}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Ngày Sinh: {(patient.DateOfBirth.HasValue ? patient.DateOfBirth.Value.ToString("dd/MM/yyyy") : "N/A")}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Địa Chỉ: {patient.Address ?? "N/A"}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Nhóm Máu: {GetBloodTypeDisplay(patient.BloodType)}")
                    .FontSize(12);
                
                doc.InsertParagraph($"SĐT: {patient.Phone ?? booking.PatientPhoneNumber ?? "N/A"}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Email: {patient.Email ?? booking.PatientEmail ?? "N/A"}")
                    .FontSize(12)
                    .SpacingAfter(20);
            }
            else
            {
                // Fallback nếu không lấy được thông tin patient
                doc.InsertParagraph($"Họ Tên: {booking.PatientName}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Giới Tính: N/A")
                    .FontSize(12);
                
                doc.InsertParagraph($"Ngày Sinh: N/A")
                    .FontSize(12);
                
                doc.InsertParagraph($"Địa Chỉ: N/A")
                    .FontSize(12);
                
                doc.InsertParagraph($"Nhóm Máu: N/A")
                    .FontSize(12);
                
                doc.InsertParagraph($"SĐT: {booking.PatientPhoneNumber ?? "N/A"}")
                    .FontSize(12);
                
                doc.InsertParagraph($"Email: {booking.PatientEmail ?? "N/A"}")
                    .FontSize(12)
                    .SpacingAfter(20);
            }

            // KẾT QUẢ XÉT NGHIỆM
            foreach (var catalog in testResult.Catalogs)
            {
                doc.InsertParagraph($"\n{catalog.CatalogName}")
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
            
            // Footer - Ngày tạo báo cáo
            doc.InsertParagraph($"\n\nNgày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .FontSize(10)
                .Italic()
                .Alignment = Xceed.Document.NET.Alignment.right;
            
            using (var ms = new MemoryStream())
            {
                doc.SaveAs(ms);
                return ms.ToArray();
            }
        }

        private async Task<PatientDetailDto?> GetPatientByIdAsync(Guid patientId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PatientApi");
                Console.WriteLine($"🔍 Calling Patient API: {client.BaseAddress}v1/patients/internal/{patientId}");
                
                var response = await client.GetAsync($"v1/patients/internal/{patientId}");
                
                Console.WriteLine($"📡 Patient API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var patient = await response.Content.ReadFromJsonAsync<PatientDetailDto>();
                    Console.WriteLine($"✅ Successfully retrieved patient data: {patient?.FullName}");
                    return patient;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Patient API Error: {response.StatusCode} - {errorContent}");
                }
                
                return null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ HTTP Request Error calling Patient API: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error calling Patient API: {ex.Message}");
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

    // DTO classes - mapping với Patient service
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

    // BloodType enum - mapping với Patient.Domain.Enums.BloodType
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
