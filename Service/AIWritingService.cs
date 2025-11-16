using System.Net.Http;
using System.Text;
using System.Text.Json;
using Repository.DAO;
using Repository.Models;

namespace AI.Services
{
    public class AIWritingService
    {
        private readonly HttpClient _http;
        private readonly string _openAiKey;

        public AIWritingService(int userId, APIKeyDAO apiKeyDao)
        {
            _http = new HttpClient();
            
            var apiKey = apiKeyDao.GetApiKeyAsync(userId).Result
                        ?? throw new Exception("User has no API key configured.");

            _openAiKey = apiKey.ChatGptkey ?? throw new Exception("ChatGPT API Key is empty.");
        }

        // ======================
        // 🔹 1️⃣ Sinh đề Writing Task 2
        // ======================               
        public async Task<string> GenerateWritingPromptAsync()
        {
            var prompt = @"
You are an IELTS Writing Task 2 question generator.
Generate ONE IELTS Writing Task 2 topic, formatted strictly as JSON string:
{""Content"":""<the question here>""}
The question must be realistic, academic, 1–2 sentences only, in English.
";

            var jsonResponse = await CallOpenAIAsync(prompt);
            try
            {
                var doc = JsonDocument.Parse(jsonResponse);
                return doc.RootElement.GetProperty("Content").GetString() ?? "Nothing was generated";
            }
            catch
            {
                return "Failed to generate topic, try later";
            }
        }

        // ======================
        // 🔹 2️⃣ Chấm điểm Writing bài làm
        // ======================
        public async Task<(decimal overall, decimal task, decimal coherence, decimal lexical, decimal grammar, string feedback)>
            GradeWritingAsync(string essay)
        {
            var gradingPrompt = $@"
You are an IELTS Writing examiner. 
Evaluate the following essay and respond only with valid JSON:
{{
  ""score"": <overall 0–9>,
  ""TaskResponse"": <0–9>,
  ""Coherence"": <0–9>,
  ""LexicalResource"": <0–9>,
  ""Grammar"": <0–9>,
  ""feedback"": ""<2–3 sentences feedback>""
}}
Essay: {essay}
";

            var jsonResponse = await CallOpenAIAsync(gradingPrompt);
            var pureJson = ExtractJsonString(jsonResponse);
            
            try
            {
                var doc = JsonDocument.Parse(pureJson);
                return (
                    doc.RootElement.GetProperty("score").GetDecimal(),
                    doc.RootElement.GetProperty("TaskResponse").GetDecimal(),
                    doc.RootElement.GetProperty("Coherence").GetDecimal(),
                    doc.RootElement.GetProperty("LexicalResource").GetDecimal(),
                    doc.RootElement.GetProperty("Grammar").GetDecimal(),
                    doc.RootElement.GetProperty("feedback").GetString() ?? ""
                );
            }
            catch
            {
                return (6.0m, 6.5m, 6.0m, 6.5m, 6.0m, "Default fallback: parsing failed.");
            }
        }

        // ======================
        // 🔹 Helper: gọi OpenAI API
        // ======================
        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[] { new { role = "user", content = prompt } },
                temperature = 0.7
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiKey);

            var res = await _http.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";
        }

        private static string ExtractJsonString(string raw)
        {
            var match = System.Text.RegularExpressions.Regex.Match(raw, @"\{[\s\S]*\}");
            return match.Success ? match.Value : "{}";
        }
    }
}
