using AI_stats_measurement.Backend.Models;

namespace AI_stats_measurement.Backend.Interface
{
    public interface ILlmAggregator
    {
        Task<List<ModelResponse>> AskByPromptIdsAsync(List<int> promptIds, List<string> modelNames, Guid jobId, CancellationToken ct);
    }
}
