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
        public ActionResult GetMetrics([FromBody] MetricsFilterDto filter)
        {
            var factsQuery = _context.FactCheckResults
                .Include(f => f.ParsedModelResponse)
                    .ThenInclude(p => p.ModelResponse)
                        .ThenInclude(m => m.Prompt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Theme))
            {
                factsQuery = factsQuery.Where(f =>
                    f.ParsedModelResponse.ModelResponse.Prompt.Theme == filter.Theme);
            }

            if (!string.IsNullOrWhiteSpace(filter.Llm))
            {
                factsQuery = factsQuery.Where(f =>
                    f.ParsedModelResponse.ModelResponse.Provider == filter.Llm);
            }

            if (!string.IsNullOrWhiteSpace(filter.Nsi))
            {
                factsQuery = factsQuery.Where(f =>
                    f.ParsedModelResponse.ModelResponse.Prompt.Provider == filter.Nsi);
            }

            var facts = factsQuery.ToList();

            var metrics = _analyticsService.GetMetrics(
                facts,
                filter.Nsi,
                filter.Llm,
                filter.Theme
            );

            return Ok(metrics);
        }
    }
}
