using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_stats_measurement.Backend.Dto
{
    public class MetricsFilterDto
    {
        public List<string>? Nsis { get; set; }
        public List<string>? Llms { get; set; }
        public List<string>? Themes { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
