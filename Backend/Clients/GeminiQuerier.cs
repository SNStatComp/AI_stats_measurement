using AI_stats_measurement.Interface;
using Google.GenAI;
using System.Text.Json;


namespace AI_stats_measurement.Clients
{
    public class GeminiQuerier : ILlmQuerier
    {
        private readonly Client _client;

        public string Name => "gemini-2.5-flash-lite-preview-09-2025";

        public GeminiQuerier(IConfiguration config)
        {
            _client = new Client(apiKey: config["LlmKeys:Gemini"]);
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            string systemMessage = prompt.Instruction;
            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash-lite-preview-09-2025",
                contents: systemMessage + prompt.Question,
                cancellationToken: ct
            );

            return response.Candidates[0].Content.Parts[0].Text ?? "";
        }
    }

}
