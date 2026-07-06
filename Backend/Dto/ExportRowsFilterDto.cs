namespace AI_stats_measurement.Backend.Dto
{
    public class ExportRowsFilterDto
    {
        public int? PromptId { get; set; }
        public List<string>? Nsis { get; set; }
        public List<string>? Themes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
