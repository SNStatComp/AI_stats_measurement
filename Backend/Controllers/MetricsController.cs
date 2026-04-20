using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly AIMeasureDbContext _context;
        private readonly AnalyticsService _analyticsService;

        public MetricsController(
            AIMeasureDbContext context,
            AnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        [HttpPost]
        public ActionResult<List<DashboardMetricsByNsiDto>> GetMetrics([FromBody] MetricsFilterDto filter)
        {
            var factsQuery = _context.FactCheckResults
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ParsedModelResponseSources)
                        .ThenInclude(pmrs => pmrs.Source)
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ModelResponse)
                        .ThenInclude(mr => mr.Prompt)
                .AsQueryable();

            // Filter out records with exceptions in the model response
            factsQuery = factsQuery.Where(f => f.ParsedModelResponse.ModelResponse.Exception == null);

            var facts = factsQuery.ToList();

            var metrics = _analyticsService.GetMetricsPerNsi(
                facts,
                filter.Nsi,
                filter.Llm,
                filter.Theme
            );

            return Ok(metrics);
        }
    }
}
