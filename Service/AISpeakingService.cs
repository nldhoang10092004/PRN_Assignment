using Deepgram;
using Deepgram.Clients.Interfaces.v1;
using Deepgram.Clients.Listen.v1.REST;
using Deepgram.Models.Listen.v1.REST;

using Repository.DAO;
using Repository.Models;

using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Service
{
    public class AISpeakingService
    {
        private readonly TranscriptionModule _transcriber;
        private readonly PromptModule _prompt;
        private readonly GradingModule _grader;

        public AISpeakingService(int userId, APIKeyDAO apiKeyDao)
        {
            var apiKey = apiKeyDao.GetApiKeyAsync(userId).Result
                        ?? throw new Exception("User has no API key configured.");

            _transcriber = new TranscriptionModule(apiKey.DeepgramKey);
            _prompt = new PromptModule(apiKey.ChatGptkey);
            _grader = new GradingModule(apiKey.ChatGptkey);
        }

        public async Task<string> TranscribeAsync(byte[] fileBytes)
            => await _transcriber.TranscribeAsync(fileBytes);

        public async Task<(string title, string content)> GenerateSpeakingPromptAsync()
            => await _prompt.GenerateSpeakingPromptAsync();

        public async Task<(decimal overall, decimal fluency, decimal lexical, decimal grammar, decimal pronunciation, string feedback)>
            GradeSpeakingAsync(string transcript, string topic)
            => await _grader.GradeSpeakingAsync(transcript, topic);

        //Deepgram
        public class TranscriptionModule
        {
            private readonly IListenRESTClient _deepgramClient;

            public TranscriptionModule(string deepgramKey)
            {
                Library.Initialize();

                if (string.IsNullOrWhiteSpace(deepgramKey))
                    throw new Exception("Deepgram API Key is empty.");

                _deepgramClient = ClientFactory.CreateListenRESTClient(deepgramKey);
            }

            public async Task<string> TranscribeAsync(byte[] audio)
            {
                if (audio == null || audio.Length == 0)
                    throw new Exception("Invalid audio input.");

                var res = await _deepgramClient.TranscribeFile(audio,
                    new PreRecordedSchema()
                    {
                        Model = "nova-3",
                        Language = "en",
                        SmartFormat = true,
                        Paragraphs = true
                    });

                var transcript = res.Results?
                    .Channels?.FirstOrDefault()?
                    .Alternatives?.FirstOrDefault()?
                    .Transcript ?? "";

                return transcript;
            }

            ~TranscriptionModule()
            {
                Library.Terminate();
            }
        }

//ChatGPT
        public class PromptModule
        {
            private readonly HttpClient _http;
            private readonly string _openAiKey;

            public PromptModule(string openAiKey)
            {
                _http = new HttpClient();
                _openAiKey = openAiKey ?? throw new Exception("ChatGPT API Key is empty.");
            }

            public async Task<(string title, string content)> GenerateSpeakingPromptAsync()
            {
                var systemPrompt = @"
You are an IELTS Speaking Part 2 question generator.
Generate ONE realistic IELTS Speaking Part 2 topic formatted strictly as JSON string:
{
  ""Title"": ""IELTS Speaking Part 2 - AI Gen"",
  ""Content"": ""<the topic card question here>""
}
";

                var jsonResponse = await CallOpenAIAsync(systemPrompt);

                try
                {
                    var doc = JsonDocument.Parse(jsonResponse);
                    return (
                        doc.RootElement.GetProperty("Title").GetString() ?? "IELTS Speaking Part 2 - AI Gen",
                        doc.RootElement.GetProperty("Content").GetString() ?? ""
                    );
                }
                catch
                {
                    return ("IELTS Speaking Part 2 - AI Gen",
                        "Describe a memorable event in your life and explain why it was special.");
                }
            }

            private async Task<string> CallOpenAIAsync(string prompt)
            {
                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.6
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
        }


        //Grading
        public class GradingModule
        {
            private readonly HttpClient _http;
            private readonly string _openAiKey;

            public GradingModule(string openAiKey)
            {
                _http = new HttpClient();
                _openAiKey = openAiKey ?? throw new Exception("ChatGPT API Key is empty.");
            }

            public async Task<(decimal overall, decimal fluency, decimal lexical,
                               decimal grammar, decimal pronunciation, string feedback)>
                GradeSpeakingAsync(string transcript, string topic)
            {
                var gradingPrompt = $@"
You are an IELTS Speaking examiner.
Respond only with valid JSON:
{{
  ""score"": <0-9>,
  ""Fluency"": <0-9>,
  ""LexicalResource"": <0-9>,
  ""Grammar"": <0-9>,
  ""Pronunciation"": <0-9>,
  ""feedback"": ""<3 sentences>""
}}
Transcript: {transcript}
Topic: {topic}";

                var jsonResponse = await CallOpenAIAsync(gradingPrompt);
                var extractedJson = ExtractJson(jsonResponse);

                try
                {
                    var doc = JsonDocument.Parse(extractedJson);
                    return (
                        doc.RootElement.GetProperty("score").GetDecimal(),
                        doc.RootElement.GetProperty("Fluency").GetDecimal(),
                        doc.RootElement.GetProperty("LexicalResource").GetDecimal(),
                        doc.RootElement.GetProperty("Grammar").GetDecimal(),
                        doc.RootElement.GetProperty("Pronunciation").GetDecimal(),
                        doc.RootElement.GetProperty("feedback").GetString() ?? ""
                    );
                }
                catch
                {
                    return (6.0m, 6.0m, 6.0m, 6.0m, 6.0m, "Default fallback: JSON parsing failed.");
                }
            }

            private async Task<string> CallOpenAIAsync(string prompt)
            {
                var payload = new
                {
                    model = "gpt-4o-mini",
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.6
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

            private static string ExtractJson(string raw)
            {
                var match = System.Text.RegularExpressions.Regex.Match(raw, @"\{[\s\S]*\}");
                return match.Success ? match.Value : "{}";
            }
        }
    }
}
