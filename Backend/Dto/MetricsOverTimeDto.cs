namespace AI_stats_measurement.Backend.Dto
{
    public class MetricsOverTimeDto
    {
        public List<ChartPointDto> Accuracy { get; set; } = new();
        public List<ChartPointDto> Consistency { get; set; } = new();
        public List<ChartPointDto> Findability { get; set; } = new();
    }
}
