namespace AI_stats_measurement.Backend.Models
{
    public class LlmJob
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Queued";
        public string? Error { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedUtc { get; set; }
    }
}
