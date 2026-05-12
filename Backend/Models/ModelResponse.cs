using AI_stats_measurement.Backend.Models;

namespace AI_stats_measurement.Backend.Models
{
    public class ModelResponse
    {
        public int Id { get; init; }
        public int PromptId { get; private set; }
        public Guid? JobId { get; private set; }
        public string Provider { get; private set; }
        public string? RawText { get; private set; }
        public string? Exception { get; private set; }
        public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;

        public Prompt Prompt { get; set; } = null!;
        public ParsedModelResponse? ParsedResponse { get; set; }
        public LlmJob? Job { get; set; }

        private ModelResponse() { }

        public ModelResponse(int promptId, string provider, string? rawText, string? exception, Guid? jobId = null)
        {
            PromptId = promptId;
            Provider = provider;
            RawText = rawText;
            Exception = exception;
            JobId = jobId;
        }

        public static ModelResponse Import(
        int id,
        int promptId,
        string provider,
        string? rawText,
        string? exception,
        DateTime createdUtc,
        Guid? jobId = null)
        {
            return new ModelResponse
            {
                Id = id,
                PromptId = promptId,
                Provider = provider,
                RawText = rawText,
                Exception = exception,
                CreatedUtc = createdUtc.Kind == DateTimeKind.Utc
                    ? createdUtc
                    : DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc),
                JobId = jobId
            };
        }
    }
}