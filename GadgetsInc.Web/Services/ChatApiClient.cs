using System.Text;
using System.Text.Json;

namespace GadgetsInc.Web.Services;

public class ChatApiClient
{
    private readonly HttpClient _httpClient;

    public ChatApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var request = new { Message = message };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/chat/simple", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatResponse>(responseJson, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        
        return result?.Response ?? "No response received.";
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(IEnumerable<ChatMessage> messages)
    {
        var request = new { Messages = messages };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/chat", content);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line?.StartsWith("data: ") == true)
            {
                var data = line.Substring(6);
                if (data == "[DONE]")
                    yield break;

                ChatChunk? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<ChatChunk>(data, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }

                if (chunk?.Content != null)
                {
                    yield return chunk.Content;
                }
            }
        }
    }

    public record ChatMessage(string Role, string Content);
    public record ChatResponse(string Response);
    public record ChatChunk(string Content);
}