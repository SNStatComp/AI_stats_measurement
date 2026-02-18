using AI_stats_measurement.Interface;
using OpenAI;
using OpenAI.Chat;


namespace AI_stats_measurement.Clients
{
    public class ChatGPTQuerier : ILlmQuerier
    {
        private readonly OpenAIClient _client;

        public string Name => "OpenAI";

        public ChatGPTQuerier(IConfiguration config)
        {
            _client = new OpenAIClient(config["LlmKeys:OpenAI"]);
        }

        public async Task<string> AskAsync(Prompt prompt, CancellationToken ct = default)
        {
            var chat = _client.GetChatClient("gpt-4o-mini");

            var response = await chat.CompleteChatAsync(
                new ChatMessage[]
                {
                    new SystemChatMessage(
                        "Je bent ChatGPT, een behulpzame en neutrale assistent voor algemene kennisvragen.Beantwoord vragen kort en duidelijk, in correct Nederlands. Vermeld welke bron je hebt gebruikt als link."
                        ),
                new UserChatMessage(prompt.Question) 
                }             
            );

            return response.Value.Content[0].Text;
        }
    }
}
