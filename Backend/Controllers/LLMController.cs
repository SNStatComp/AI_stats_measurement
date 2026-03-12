using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Models;
using AI_stats_measurement.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("ask")]
        public async Task<ActionResult<List<ModelResponse>>> AskAll([FromBody] List<Prompt> prompt, CancellationToken ct)
        {
            if (prompt is null)
                return BadRequest("Prompt is required.");

            var results = await _llmAggregator.AskAllAsync(prompt, ct);

            return Ok(results);
        }
        [HttpPost("askById")]
        public async Task<ActionResult<List<ModelResponse>>> AskByPromptIds([FromBody] List<int> promptIds, CancellationToken ct)
        {
            if (promptIds == null || promptIds.Count == 0)
                return BadRequest("At least one PromptId is required.");

            var results = await _llmAggregator.AskByPromptIdsAsync(promptIds, ct);
            return Ok(results);
        }

        [HttpPost("run")]
        public async Task<ActionResult<List<ModelResponse>>> Run([FromBody] List<int> promptIds, CancellationToken ct)
        {
            if (promptIds == null || promptIds.Count == 0)
                return BadRequest("At least one PromptId is required.");

            var results = await _evaluationPipeline.RunAsync(promptIds, ct);
            return Ok(results);
        }      
    }
}
