using AI_stats_measurement.Interface;
using OpenAI;
using OpenAI.Chat;


namespace AI_stats_measurement.Clients
{
    public class ChatGPTQuerier : ILlmQuerier
    {
        private readonly OpenAIClient _client;

        public string Name => "gpt-4o-mini";

        public ChatGPTQuerier(IConfiguration config)
        {
            _client = new OpenAIClient(config["LlmKeys:OpenAI"]);
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var chat = _client.GetChatClient(Name);

            var response = await chat.CompleteChatAsync(
                new ChatMessage[]
                {
                    new SystemChatMessage(prompt.Instruction),
                    new UserChatMessage(prompt.Question) 
                }             
            );

            return response.Value.Content[0].Text;
        }
    }
}
