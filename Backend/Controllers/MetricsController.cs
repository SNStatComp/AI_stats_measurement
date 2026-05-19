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

        [HttpPost("analytics/metrics-per-nsi")]
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


        [HttpPost("analytics/metrics-per-model")]
        public ActionResult<List<DashboardMetricsByNsiDto>> GetMetricsPerModel([FromBody] MetricsFilterDto filter)
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

            var metrics = _analyticsService.GetMetricsPerModel(
                facts,
                filter.Nsi,
                filter.Llm,
                filter.Theme
            );

            return Ok(metrics);
        }

        [HttpPost("analytics/metrics-per-theme")]
        public ActionResult<List<DashboardMetricsByNsiDto>> GetMetricsPerTheme([FromBody] MetricsFilterDto filter)
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

            var metrics = _analyticsService.GetMetricsPerTheme(
                facts,
                filter.Nsi,
                filter.Llm,
                filter.Theme
            );

            return Ok(metrics);
        }

        [HttpPost("analytics/weekly/{groupBy}")]
        public async Task<ActionResult<Dictionary<string, MetricsOverTimeDto>>> GetWeeklyMetrics( string groupBy, [FromBody] MetricsFilterDto filter)
        {
            var facts = await _context.FactCheckResults
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ParsedModelResponseSources)
                        .ThenInclude(s => s.Source)
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ModelResponse)
                        .ThenInclude(mr => mr.Prompt)
                .Where(f => f.ParsedModelResponse.ModelResponse.Exception == null)
                .ToListAsync();

            var result = groupBy.ToLower() switch
            {
                "nsi" => _analyticsService.GetWeeklyMetricsPerNsi(
                    facts, filter.Nsi, filter.Llm, filter.Theme),

                "model" => _analyticsService.GetWeeklyMetricsPerModel(
                    facts, filter.Nsi, filter.Llm, filter.Theme),

                "theme" => _analyticsService.GetWeeklyMetricsPerTheme(
                    facts, filter.Nsi, filter.Llm, filter.Theme),

                _ => null
            };

            if (result == null)
            {
                return BadRequest("Invalid groupBy value. Use: nsi, model, or theme.");
            }

            return Ok(result);
        }
    }
}
