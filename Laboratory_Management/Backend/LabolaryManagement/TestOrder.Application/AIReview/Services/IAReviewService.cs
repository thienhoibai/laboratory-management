using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using TestOrder.Application.AIReview.DTOs;
using TestOrder.Application.AIReview.Interfaces;

namespace TestOrder.Application.AIReview.Services;

/// <summary>
/// Service gọi Gemini AI để phân tích kết quả xét nghiệm
/// </summary>
public class IAReviewService : IIAReviewService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;

    public IAReviewService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(90); // Tăng timeout

        // Đọc API Key từ appsettings.json
        var rawKey = configuration["AiSettings:ApiKey"];
        _apiKey = rawKey?.Trim() ?? throw new ArgumentNullException("AiSettings:ApiKey", "Gemini API Key is missing in appsettings.json");

        // Đọc Model Name từ appsettings.json
        var rawModel = configuration["AiSettings:ModelName"];
        _modelName = string.IsNullOrWhiteSpace(rawModel) ? "gemini-2.0-flash-exp" : rawModel.Trim();
    }

    public async Task<List<TestResultDetail>> AnalyzeTestResultAsync(List<TestInputItem> testResults)
    {
        var responseList = new List<TestResultDetail>();

        // --- BƯỚC 1: Map dữ liệu (Không tính toán lại Status) ---
        foreach (var item in testResults)
        {
            responseList.Add(new TestResultDetail
            {
                Parameter = item.Parameter,
                Value = item.Value,
                Range = item.Range,
                Unit = item.Unit ?? "",
                Status = item.Status,
                Comment = "..."
            });
        }

        // --- BƯỚC 2: Chuẩn bị Prompt cho Gemini AI ---
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Bạn là bác sĩ chuyên khoa. Hãy viết nhận xét dựa trên trạng thái (Status) đã được cung cấp dưới đây:");
        promptBuilder.AppendLine("--- KẾT QUẢ XÉT NGHIỆM ---");

        for (int i = 0; i < responseList.Count; i++)
        {
            var item = responseList[i];
            var inputItem = testResults[i];
            
            string rangeInfo = item.Range;
            if (inputItem.MinRange.HasValue && inputItem.MaxRange.HasValue)
            {
                string deviation = "";
                if (double.TryParse(item.Value, out var value))
                {
                    if (item.Status == "Low")
                    {
                        var diff = inputItem.MinRange.Value - value;
                        var percent = (diff / inputItem.MinRange.Value) * 100;
                        deviation = $" (Thấp hơn {diff:F2}, chênh lệch {percent:F1}%)";
                    }
                    else if (item.Status == "High")
                    {
                        var diff = value - inputItem.MaxRange.Value;
                        var percent = (diff / inputItem.MaxRange.Value) * 100;
                        deviation = $" (Cao hơn {diff:F2}, chênh lệch {percent:F1}%)";
                    }
                }
                rangeInfo = $"{item.Range} [Min: {inputItem.MinRange.Value}, Max: {inputItem.MaxRange.Value}]{deviation}";
            }

            // ✅ QUAN TRỌNG: Dùng tên parameter CHÍNH XÁC
            promptBuilder.AppendLine($"- {item.Parameter}: {item.Value} {item.Unit} (Khoảng tham chiếu: {rangeInfo}) → TRẠNG THÁI: {item.Status}");
        }

        promptBuilder.AppendLine("\n--- YÊU CẦU OUTPUT ---");
        promptBuilder.AppendLine("Hãy viết nhận xét ngắn gọn (1-2 câu, tối đa 80 từ) cho TỪNG chỉ số:");
        promptBuilder.AppendLine("1. Giải thích ý nghĩa và tình trạng sức khỏe");
        promptBuilder.AppendLine("2. Nguyên nhân có thể (chỉ với Low/High)");
        promptBuilder.AppendLine("3. Lời khuyên cụ thể về dinh dưỡng và sinh hoạt");
        promptBuilder.AppendLine("4. Giọng văn: Ân cần, chuyên nghiệp, dễ hiểu");

        promptBuilder.AppendLine("\n--- ĐỊNH DẠNG JSON (BẮT BUỘC) ---");
        promptBuilder.AppendLine("Trả về JSON Object phẳng. Key = Tên parameter CHÍNH XÁC (y hệt như trên), Value = Nhận xét.");
        promptBuilder.AppendLine("QUY TẮC JSON:");
        promptBuilder.AppendLine("1. KHÔNG dùng dấu ngoặc kép (\") trong nội dung nhận xét");
        promptBuilder.AppendLine("2. KHÔNG xuống dòng (\\n) - viết liền một dòng");
        promptBuilder.AppendLine("3. Dùng dấu ngoặc đơn (') nếu cần");
        promptBuilder.AppendLine("4. Mỗi nhận xét NGẮN GỌN, tối đa 80 từ, không xuống dòng");
        promptBuilder.AppendLine("5. Key phải GIỐNG Y HỆT tên parameter (ví dụ: 'Hồng cầu (RBC)', KHÔNG thêm 'Chỉ số')");
        promptBuilder.AppendLine("Ví dụ ĐÚNG: { \"Hồng cầu (RBC)\": \"Chỉ số hồng cầu 4.32 nằm trong khoảng bình thường, phản ánh khả năng vận chuyển oxy tốt. Hãy duy trì chế độ ăn cân bằng giàu sắt.\" }");
        promptBuilder.AppendLine("Ví dụ SAI: { \"Chỉ số Hồng cầu (RBC)\": \"...\" } ← TUYỆT ĐỐI KHÔNG THÊM 'Chỉ số'");

        // --- BƯỚC 3: Gọi Gemini API ---
        var requestBody = new
        {
            contents = new[] 
            { 
                new 
                { 
                    parts = new[] 
                    { 
                        new { text = promptBuilder.ToString() } 
                    } 
                } 
            },
            generationConfig = new 
            { 
                responseMimeType = "application/json",
                temperature = 0.7,
                maxOutputTokens = 4096  // ✅ TĂNG LÊN ĐỂ KHÔNG BỊ CẮT
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent?key={_apiKey}";
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();

            var jsonResponseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponseString);

            string aiJsonText = doc.RootElement.GetProperty("candidates")[0]
                                               .GetProperty("content")
                                               .GetProperty("parts")[0]
                                               .GetProperty("text")
                                               .GetString() ?? "{}";

            // Clean JSON
            if (!string.IsNullOrEmpty(aiJsonText))
            {
                aiJsonText = aiJsonText.Replace("```json", "").Replace("```", "").Trim();
            }

            Dictionary<string, string>? aiComments = null;

            try
            {
                aiComments = JsonSerializer.Deserialize<Dictionary<string, string>>(aiJsonText);
            }
            catch (JsonException)
            {
                // Fallback: Try to fix JSON by removing newlines in values
                try
                {
                    var fixedJson = System.Text.RegularExpressions.Regex.Replace(
                        aiJsonText, 
                        @"(?<="")([^""]*?)(?="")",
                        m => m.Value.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ")
                    );
                    
                    aiComments = JsonSerializer.Deserialize<Dictionary<string, string>>(fixedJson);
                }
                catch
                {
                    // If still fails, leave aiComments as null
                }
            }

            // --- BƯỚC 4: Gán Comment vào từng chỉ số ---
            foreach (var item in responseList)
            {
                if (aiComments != null && aiComments.ContainsKey(item.Parameter))
                {
                    item.Comment = aiComments[item.Parameter];
                }
                else
                {
                    // Fuzzy matching
                    string? matchedKey = null;
                    
                    if (aiComments != null)
                    {
                        var paramWithoutParentheses = item.Parameter.Split('(')[0].Trim();
                        
                        // 1. Exact match without parentheses
                        matchedKey = aiComments.Keys.FirstOrDefault(k => 
                            k.Equals(paramWithoutParentheses, StringComparison.OrdinalIgnoreCase)
                        );
                        
                        // 2. Contains match
                        if (matchedKey == null)
                        {
                            matchedKey = aiComments.Keys.FirstOrDefault(k => 
                                k.Contains(paramWithoutParentheses, StringComparison.OrdinalIgnoreCase) ||
                                paramWithoutParentheses.Contains(k, StringComparison.OrdinalIgnoreCase)
                            );
                        }
                        
                        // 3. Code in parentheses
                        if (matchedKey == null && item.Parameter.Contains('('))
                        {
                            var codeInParentheses = item.Parameter.Split('(')[1].Replace(")", "").Trim();
                            matchedKey = aiComments.Keys.FirstOrDefault(k => 
                                k.Contains(codeInParentheses, StringComparison.OrdinalIgnoreCase)
                            );
                        }
                    }
                    
                    if (matchedKey != null && aiComments != null)
                    {
                        item.Comment = aiComments[matchedKey];
                    }
                    else
                    {
                        item.Comment = "Không có nhận xét từ AI cho chỉ số này.";
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            foreach (var item in responseList) 
                item.Comment = $"Lỗi kết nối AI: {ex.Message}";
        }
        catch (Exception ex)
        {
            foreach (var item in responseList) 
                item.Comment = $"Lỗi AI: {ex.Message}";
        }

        return responseList;
    }
}
