using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_stats_measurement.Backend.Dto
{
    public class MetricsFilterDto 
    {
        public string? Nsi { get; set; }
        public string? Llm { get; set; }
        public string? Theme { get; set; }
    }
}
