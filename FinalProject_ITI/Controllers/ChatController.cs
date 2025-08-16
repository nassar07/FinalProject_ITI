using FinalProject_ITI.Models;
using FinalProject_ITI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mscc.GenerativeAI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly EmbeddingService _embeddingService;
        private readonly GoogleAI _googleAI;
        private readonly ILogger<ChatController> _logger;
        private const int CharBudget = 3500;

        private const string GeminiKey = "<GEMINI_KEY>";
        private const string DeepSeekKey = "sk-2a161dea47ca4cc79d35759e4155e701";

        public ChatController(EmbeddingService embeddingService, GoogleAI googleAI, ILogger<ChatController> logger)
        {
            _embeddingService = embeddingService;
            _googleAI = googleAI;
            _logger = logger;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processing chat question: {Q}", request.Question);

                var chunks = await _embeddingService.GetTopKContextChunksAsync(request.Question, k: 8);
                var sources = new List<string>();

                string prompt;
                if (chunks != null && chunks.Any())
                {
                    var sb = new StringBuilder();
                    foreach (var c in chunks.OrderByDescending(x => x.Score))
                    {
                        if (sb.Length + c.Text.Length + 200 > CharBudget) break;
                        sb.AppendLine($"[Source: {c.Source}] (score: {c.Score:F3})");
                        sb.AppendLine(c.Text);
                        sb.AppendLine();
                        if (!sources.Contains(c.Source)) sources.Add(c.Source);
                    }
                    prompt = BuildPromptWithContext(sb.ToString(), request.Question);
                }
                else
                {
                    prompt = BuildPromptForNoContext(request.Question);
                }

                // جرّب Gemini أولاً
                try
                {
                    var answer = await AskGemini(prompt);
                    return Ok(new { Answer = answer, Sources = sources });
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("402") || ex.Message.Contains("quota"))
                {
                    _logger.LogWarning("Gemini quota ended. Switching to DeepSeek...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Gemini failed, switching to DeepSeek...");
                }

                // جرّب DeepSeek
                var deepSeekAnswer = await AskDeepSeek(prompt);
                return Ok(new { Answer = deepSeekAnswer, Sources = sources });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Ask");
                return StatusCode(500, new { Error = "حدث خطأ أثناء المعالجة", Details = ex.Message });
            }
        }

        private async Task<string> AskGemini(string prompt)
        {
            var model = _googleAI.GenerativeModel(model: "gemini-1.5-flash");
            var resp = await model.GenerateContent(prompt);
            return resp.Text;
        }

        private async Task<string> AskDeepSeek(string prompt)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", DeepSeekKey);

            var body = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "أنت مساعد ذكي ويجب أن تجيب باللغة العربية فقط." },
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);

            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }

        private string BuildPromptWithContext(string context, string question)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a helpful assistant that MUST answer in Arabic.");
            sb.AppendLine("Use ONLY the provided CONTEXT to answer the user's question. Do not invent facts. If the answer is not present in the context, respond: \"لا توجد معلومات كافية في المصادر\".");
            sb.AppendLine("When you use information from the context, mention the source in square brackets like: [المصدر: <source>].");
            sb.AppendLine();
            sb.AppendLine("CONTEXT:");
            sb.AppendLine(context);
            sb.AppendLine();
            sb.AppendLine($"USER QUESTION: {question}");
            sb.AppendLine();
            sb.AppendLine("ANSWER:");
            return sb.ToString();
        }

        private string BuildPromptForNoContext(string question)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a helpful assistant that MUST answer in Arabic.");
            sb.AppendLine("Be honest: if you don't know the answer, say \"لا توجد معلومات كافية\".");
            sb.AppendLine();
            sb.AppendLine($"USER QUESTION: {question}");
            sb.AppendLine();
            sb.AppendLine("ANSWER:");
            return sb.ToString();
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}
