namespace AI_stats_measurement.Interface
{
    public interface ILlmQuerier
    {
        string Name { get;}

        Task<string> AskAsync(Prompt prompt, CancellationToken ct = default);
    }
}
