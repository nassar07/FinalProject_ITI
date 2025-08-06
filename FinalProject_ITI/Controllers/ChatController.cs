using FinalProject_ITI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mscc.GenerativeAI;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly EmbeddingService _embeddingService;
        private readonly GoogleAI _googleAI;
        private readonly ILogger<ChatController> _logger;

        public ChatController(EmbeddingService embeddingService, IConfiguration configuration, ILogger<ChatController> logger)
        {
            _embeddingService = embeddingService;
            _logger = logger;
            var apiKey = configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Gemini API Key is not configured");
                throw new InvalidOperationException("Gemini API Key is not configured");
            }
            _googleAI = new GoogleAI(apiKey);
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processing chat request: {Question}", request.Question);

                var context = await _embeddingService.FindMostRelevantContext(request.Question);
                var prompt = $"You are an AI assistant. Answer the user's question based ONLY on the provided context.\n\nCONTEXT:\n{context}\n\nQUESTION: {request.Question}\n\nANSWER:";

                _logger.LogInformation("Using model:gemini-1.5-flash");
                var generativeModel = _googleAI.GenerativeModel(model: "gemini-1.5-flash");
                var response = await generativeModel.GenerateContent(prompt);

                _logger.LogInformation("Successfully generated response");
                return Ok(new { Answer = response.Text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new { Error = "An error occurred while processing your request", Details = ex.Message });
            }
        }

        [HttpPost("index-data")]
        public async Task<IActionResult> IndexData()
        {
            try
            {
                await _embeddingService.IndexAllProducts();
                return Ok("Indexing completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during indexing");
                return StatusCode(500, new { Error = "An error occurred during indexing", Details = ex.Message });
            }
        }

        [HttpPost("index-all-data")]
        public async Task<IActionResult> IndexAllData()
        {
            try
            {
                _logger.LogInformation("Starting comprehensive data indexing");
                await _embeddingService.IndexAllData();
                return Ok("Comprehensive data indexing completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during comprehensive indexing");
                return StatusCode(500, new { Error = "An error occurred during comprehensive indexing", Details = ex.Message });
            }
        }

        [HttpGet("test-api")]
        public async Task<IActionResult> TestApi()
        {
            try
            {
                _logger.LogInformation("Testing Gemini API connection");
                var generativeModel = _googleAI.GenerativeModel(model: "gemini-1.5-flash");
                var response = await generativeModel.GenerateContent("Hello, this is a test message.");
                return Ok(new { Success = true, Response = response.Text });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API test failed");
                return StatusCode(500, new { Error = "API test failed", Details = ex.Message });
            }
        }
    }

    public class ChatRequest { public string Question { get; set; } = string.Empty; }
}
