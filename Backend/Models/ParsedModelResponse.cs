namespace AI_stats_measurement.Models
{
    public class ParsedModelResponse
    {
        public int Id { get; init; }
        public int ModelResponseId { get; private set; }
        public decimal Answer { get; private set; }
        public List<string> Sources { get; private set; } = new();

        public ModelResponse ModelResponse { get; set; } = null!;
        public FactCheckResult? FactCheckResult { get; set; }

        private ParsedModelResponse() { }

        public ParsedModelResponse(int modelResponseId, decimal answer, List<string> sources)
        {
            ModelResponseId = modelResponseId;
            Answer = answer;
            Sources = sources;
        }
    }
}
