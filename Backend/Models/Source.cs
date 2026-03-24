namespace AI_stats_measurement.Backend.Models
{
    public class Source
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }

        public List<Prompt> Prompts { get; set; } = new();
        public List<ParsedModelResponseSource> ParsedModelResponseSources { get; set; } = new();
    }
}
