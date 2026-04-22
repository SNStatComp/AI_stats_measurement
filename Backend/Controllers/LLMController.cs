using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Models;
using AI_stats_measurement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AI_stats_measurement.Controllers
{
    [ApiController]
    [Route("api/llm")]
    public class LlmController : ControllerBase
    {
        private readonly LlmAggregator _llmAggregator;
        private readonly EvaluationPipeline _evaluationPipeline;

        public LlmController(LlmAggregator llmAggregator, EvaluationPipeline evaluationPipeline)
        {
            _llmAggregator = llmAggregator;
            _evaluationPipeline = evaluationPipeline;
        }

        [Authorize]
        [HttpPost("run")]
        public async Task<ActionResult<List<ModelResponse>>> Run([FromBody] RunRequest request, CancellationToken ct)
        {
            if (request.PromptIds == null || request.PromptIds.Count == 0)
                return BadRequest("At least one PromptId is required.");

            var results = await _evaluationPipeline.RunAsync(request.PromptIds, request.ModelNames, ct);
            return Ok(results);
        }

        [Authorize]
        [HttpPost("recalculate")]
        public async Task<ActionResult<List<ExportRow>>> Recalculate([FromBody] List<int> promptIds, CancellationToken ct)
        {
            var result = await _evaluationPipeline.RecalculateAsync(ct);
            return Ok(result);
        }

        public class RunRequest
        {
            public List<int> PromptIds { get; set; } = new();
            public List<string> ModelNames { get; set; } = new();
        }
    }
}
