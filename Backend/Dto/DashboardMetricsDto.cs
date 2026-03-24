namespace AI_stats_measurement.Backend.Dto
{
    public class DashboardMetricsDto
    {
        public double AccuracyScore { get; set; }
        public double ConsistencyScore { get; set; }
        public double FindabilityScore { get; set; }
        public int TotalMeasurements { get; set; }
    }
}
