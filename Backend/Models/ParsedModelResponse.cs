using AI_stats_measurement.Backend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_stats_measurement.Models
{
    public class ParsedModelResponse
    {
        public int Id { get; set; }
        public int ModelResponseId { get; set; }
        public decimal Answer { get; set; }

        public ModelResponse ModelResponse { get; set; } = null!;
        public FactCheckResult? FactCheckResult { get; set; }

        public List<ParsedModelResponseSource> ParsedModelResponseSources { get; set; } = new();

        [NotMapped]
        public List<ExtractedSource> ExtractedSources { get; set; } = new();

        private ParsedModelResponse() { }

        public ParsedModelResponse(int modelResponseId, decimal answer, List<ExtractedSource> extractedSources)
        {
            ModelResponseId = modelResponseId;
            Answer = answer;
            ExtractedSources = extractedSources;
        }
    }

    public class ExtractedSource
    {
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
