namespace AI_stats_measurement.Backend.Dto;
public class DashboardMetricsByNsiDto
{
    public string Nsi { get; set; } = string.Empty;
    public double AccuracyScore { get; set; }
    public double ConsistencyScore { get; set; }
    public double FindabilityScore { get; set; }
    public int TotalMeasurements { get; set; }


    public string ConsistencyScoreTooltip { get; set; } = string.Empty;
    public string AccuracyScoreTooltip { get; set; } = string.Empty;
    public string FindabilityScoreTooltip { get; set; } = string.Empty;


    public List<SourceCount> TopSources { get; set; } = new();


}

public class SourceCount
{
    public string Hostname { get; set; } = string.Empty;
    public int Count { get; set; }
}