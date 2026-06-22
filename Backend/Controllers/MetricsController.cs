using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;
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

            PeriodeFilter(factsQuery, filter.StartDate, filter.EndDate);

            // Filter out records with exceptions in the model response
            factsQuery = factsQuery.Where(f => f.ParsedModelResponse.ModelResponse.Exception == null);

            var facts = factsQuery.ToList();

            var metrics = _analyticsService.GetMetricsPerNsi(
                facts,
                filter.Nsis,
                filter.Llms,
                filter.Themes
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

            PeriodeFilter(factsQuery,filter.StartDate, filter.EndDate);

            // Filter out records with exceptions in the model response
            factsQuery = factsQuery.Where(f => f.ParsedModelResponse.ModelResponse.Exception == null);

            var facts = factsQuery.ToList();

            var metrics = _analyticsService.GetMetricsPerModel(
                facts,
                filter.Nsis,
                filter.Llms,
                filter.Themes
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

            PeriodeFilter(factsQuery, filter.StartDate, filter.EndDate);

            // Filter out records with exceptions in the model response
            factsQuery = factsQuery.Where(f => f.ParsedModelResponse.ModelResponse.Exception == null);

            var facts = factsQuery.ToList();

            var metrics = _analyticsService.GetMetricsPerTheme(
                facts,
                filter.Nsis,
                filter.Llms,
                filter.Themes
            );

            return Ok(metrics);
        }

        [HttpPost("analytics/weekly/{groupBy}")]
        public async Task<ActionResult<Dictionary<string, MetricsOverTimeDto>>> GetWeeklyMetrics(string groupBy, [FromBody] MetricsFilterDto filter)
        {
            var factsQuery = _context.FactCheckResults
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ParsedModelResponseSources)
                        .ThenInclude(s => s.Source)
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(pmr => pmr.ModelResponse)
                        .ThenInclude(mr => mr.Prompt)
                .Where(f => f.ParsedModelResponse.ModelResponse.Exception == null)
                .AsQueryable();

            PeriodeFilter(factsQuery, filter.StartDate, filter.EndDate);

            var facts = factsQuery.ToList();

            var result = groupBy.ToLower() switch
            {
                "nsi" => _analyticsService.GetWeeklyMetricsPerNsi(
                    facts, filter.Nsis, filter.Llms, filter.Themes),

                "model" => _analyticsService.GetWeeklyMetricsPerModel(
                    facts, filter.Nsis, filter.Llms, filter.Themes),

                "theme" => _analyticsService.GetWeeklyMetricsPerTheme(
                    facts, filter.Nsis, filter.Llms, filter.Themes),

                _ => null
            };

            if (result == null)
            {
                return BadRequest("Invalid groupBy value. Use: nsi, model, or theme.");
            }

            return Ok(result);
        }


        private IQueryable<FactCheckResult> PeriodeFilter(IQueryable<FactCheckResult> factCheckResults, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
            {
                factCheckResults = factCheckResults.Where(f =>
                    f.ParsedModelResponse.ModelResponse.CreatedUtc >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endDateExclusive = endDate.Value.Date.AddDays(1);

                factCheckResults = factCheckResults.Where(f =>
                    f.ParsedModelResponse.ModelResponse.CreatedUtc < endDateExclusive);
            }

            return factCheckResults;
        }
    }
}
