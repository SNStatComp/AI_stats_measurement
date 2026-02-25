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

        public LlmController(LlmAggregator llmAggregator)
        {
            _llmAggregator = llmAggregator;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<List<ModelResponse>>> AskAll([FromBody] List<Prompt> prompt, CancellationToken ct)
        {
            if (prompt is null)
                return BadRequest("Prompt is required.");

            var results = await _llmAggregator.AskAllAsync(prompt, ct);

            return Ok(results);
        }
    }
}
