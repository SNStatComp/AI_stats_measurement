using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace AI_stats_measurement.Controllers
{
    [ApiController]
    [Route("api/llm")]
    public class LlmController : ControllerBase
    {
        private readonly EvaluationPipeline _evaluationPipeline;
        private readonly AIMeasureDbContext _context;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public LlmController(EvaluationPipeline evaluationPipeline, AIMeasureDbContext context, IServiceScopeFactory serviceScopeFactory)
        { 
            _evaluationPipeline = evaluationPipeline;
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [Authorize]
        [HttpPost("run")]
        public async Task<IActionResult> Run([FromBody] RunRequest request, CancellationToken ct)
        {
            var job = new LlmJob
            {
                Id = Guid.NewGuid(),
                Status = "Queued"
            };

            _context.LlmJobs.Add(job);
            await _context.SaveChangesAsync(ct);

            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AIMeasureDbContext>();
                var pipeline = scope.ServiceProvider.GetRequiredService<EvaluationPipeline>();

                var backgroundJob = await db.LlmJobs.FindAsync(job.Id);

                try
                {
                    backgroundJob!.Status = "Running";
                    await db.SaveChangesAsync();

                    await pipeline.RunAsync(request.PromptIds, request.ModelNames, job.Id, CancellationToken.None);

                    backgroundJob.Status = "Completed";
                    backgroundJob.FinishedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    backgroundJob!.Status = "Failed";
                    backgroundJob.Error = ex.Message;
                    backgroundJob.FinishedUtc = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            });

            return Accepted(new { jobId = job.Id });
        }

        [Authorize]
        [HttpPost("recalculate")]
        public IActionResult Recalculate(CancellationToken ct)
        {
            _ = Task.Run(async () =>
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<EvaluationPipeline>();

                await pipeline.RecalculateAsync(CancellationToken.None);
            });

            return Accepted(new { message = "Recalculation started" });
        }

        public class RunRequest
        {
            public List<int> PromptIds { get; set; } = new();
            public List<string> ModelNames { get; set; } = new();
        }
    }
}
