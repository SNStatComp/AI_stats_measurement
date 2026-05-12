namespace AI_stats_measurement.Backend.Models
{
    public class ParsedModelResponseSource
    {
        public int ParsedModelResponseId { get; set; }
        public ParsedModelResponse ParsedModelResponse { get; set; } = null!;

        public int SourceId { get; set; }
        public Source Source { get; set; } = null!;
    }
}
