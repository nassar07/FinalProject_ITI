using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.SqlClient;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly SqlConnection _dbConnection;

    public ChatService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }

    public async Task<(string answer, string source)> AskQuestionAsync(string question)
    {
        string aiAnswer;
        string source;

        try
        {
            aiAnswer = await AskGeminiAsync(question);
            source = "Gemini";
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            aiAnswer = await AskDeepSeekAsync(question);
            source = "DeepSeek";
        }
        catch
        {
            aiAnswer = "لم أتمكن من جلب الإجابة من الذكاء الاصطناعي.";
            source = "Database Only";
        }

        var dbInfo = await SearchDatabaseAsync(question);

        if (!string.IsNullOrEmpty(dbInfo))
            aiAnswer += $"\n\n📌 معلومات من قاعدة البيانات:\n- {dbInfo.Replace("\n", "\n- ")}";

        return (aiAnswer, source);
    }

    private async Task<string> AskGeminiAsync(string question)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + apiKey;

        var payload = new
        {
            contents = new[]
            {
                new {
                    parts = new[] { new { text = question } }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("candidates")[0]
                     .GetProperty("content")
                     .GetProperty("parts")[0]
                     .GetProperty("text").GetString();
    }

    private async Task<string> AskDeepSeekAsync(string question)
    {
        var apiKey = _config["DeepSeek:ApiKey"];
        var url = "https://api.deepseek.com/chat/completions";

        var payload = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "user", content = question }
            }
        };

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0]
                     .GetProperty("message")
                     .GetProperty("content").GetString();
    }

    private async Task<string> SearchDatabaseAsync(string question)
    {
        await _dbConnection.OpenAsync();
        string sql = "SELECT TOP 5 Info FROM KnowledgeBase WHERE Question LIKE @search OR Info LIKE @search";
        using var cmd = new SqlCommand(sql, _dbConnection);
        cmd.Parameters.AddWithValue("@search", "%" + question + "%");

        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<string>();

        while (await reader.ReadAsync())
            results.Add(reader.GetString(0));

        await _dbConnection.CloseAsync();

        return results.Count > 0 ? string.Join("\n", results) : string.Empty;
    }
}
