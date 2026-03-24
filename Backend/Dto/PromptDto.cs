namespace AI_stats_measurement.Backend.Dto
{
    public class PromptDto
    {
        public string Provider { get; set; } = null!;
        public string Instruction { get; set; } = null!;
        public string Theme { get; set; } = null!;
        public DateTime Periode { get; set; }
        public string Subject { get; set; } = null!;
        public string Question { get; set; } = null!;
        public decimal Answer { get; set; }
        public string SourceName { get; set; } = null!;
        public string SourceType { get; set; } = null!;
        public string SourceUrl { get; set; } = null!;
        public string AnswerLocation { get; set; } = null!;
        public Dictionary<string, string> Dimensions { get; set; } = new();
    }
}
