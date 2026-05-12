#pragma warning disable OPENAI001
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Interface;

using OpenAI.Responses;

namespace AI_stats_measurement.Backend.Clients
{
    public class ChatGPTWebSearchQuerier : ILlmQuerier
    {
        private readonly ResponsesClient _client;

        public string Name => "gpt-5.4";

        public ChatGPTWebSearchQuerier(IConfiguration config)
        {
            _client = new ResponsesClient(config["LlmKeys:OpenAI"]);
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var options = new CreateResponseOptions
            {
                Model = Name
            };

            options.Tools.Add(ResponseTool.CreateWebSearchTool());

            options.InputItems.Add(
                ResponseItem.CreateUserMessageItem(
                    $"{prompt.Instruction}\n\n{prompt.Question}")
            );

            var response = await _client.CreateResponseAsync(options, ct);

            return response.Value.GetOutputText();
        }
    }
}
