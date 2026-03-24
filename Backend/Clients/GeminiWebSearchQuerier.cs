using AI_stats_measurement.Interface;
using Google.GenAI;
using Google.GenAI.Types;

namespace AI_stats_measurement.Backend.Clients
{
    public class GeminiWebSearchQuerier : ILlmQuerier
    {
        private readonly Client _client;

        public string Name => "gemini-3.1-pro-preview";

        public GeminiWebSearchQuerier(IConfiguration config)
        {
            _client = new Client(apiKey: config["LlmKeys:Gemini"]);
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var config = new GenerateContentConfig
            {
                Tools = new List<Tool>
        {
            new Tool
            {
                GoogleSearch = new GoogleSearch()
            }
        }
            };

            var response = await _client.Models.GenerateContentAsync(
                model: Name,
                contents: $"{prompt.Instruction}\n\n{prompt.Question}",
                config: config,
                cancellationToken: ct
            );

            return response.Candidates[0].Content.Parts[0].Text ?? "";
        }
    }
}
