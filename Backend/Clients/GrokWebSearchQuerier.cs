using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Interface;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AI_stats_measurement.Backend.Clients
{
    public class GrokWebSearchQuerier : ILlmQuerier
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public string Name => "grok-4.20-reasoning";

        public GrokWebSearchQuerier(IConfiguration config)
        {
            _apiKey = config["LlmKeys:Grok"] ?? throw new InvalidOperationException("Missing Grok API key.");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.x.ai/")
            };
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = Name,
                input = new object[]
                {
                    new
                    {
                        role = "system",
                        content = prompt.Instruction
                    },
                    new
                    {
                        role = "user",
                        content = prompt.Question
                    }
                },
                tools = new object[]
                {
                    new
                    {
                        type = "web_search"
                    }
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            var jsonText = await response.Content.ReadAsStringAsync(ct);

            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (root.TryGetProperty("output", out var output))
            {
                foreach (var item in output.EnumerateArray())
                {
                    if (item.TryGetProperty("type", out var itemType) &&
                        itemType.GetString() == "message" &&
                        item.TryGetProperty("content", out var content))
                    {
                        foreach (var part in content.EnumerateArray())
                        {
                            if (part.TryGetProperty("type", out var partType) &&
                                partType.GetString() == "output_text" &&
                                part.TryGetProperty("text", out var text))
                            {
                                return text.GetString() ?? "";
                            }
                        }
                    }
                }
            }

            return "";
        }
    }
}
