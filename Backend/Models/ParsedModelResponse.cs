namespace AI_stats_measurement.Models
{
    public class ParsedModelResponse
    {
        public int Id { get; init; }
        public int ModelResponseId { get; private set; }
        public int Answer { get; private set; }
        public string? Source { get; private set; }
    }
}
