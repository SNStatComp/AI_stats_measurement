using AI_stats_measurement.Interface;
using OpenAI;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace AI_stats_measurement.Clients
{
    public class GrokQuerier : ILlmQuerier
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public string Name => "xAI";
        public GrokQuerier(IConfiguration config)
        {
            _apiKey = config["LlmKeys:Grok"];
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.x.ai/")
            };
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var body = new
            {
                model = "grok-4-1-fast-reasoning",
                temperature = 0,
                stream = false,
                messages = new[]
                {
                new { role = "system", content = "Je bent Grok, een behulpzame en neutrale assistent voor algemene kennisvragen.Beantwoord vragen kort en duidelijk, in correct Nederlands. Vermeld welke bron je hebt gebruikt als link." },
                new { role = "user", content = prompt.Question }
            }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, ct);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            return json
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }
    }

}
