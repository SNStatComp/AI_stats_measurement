using AI_stats_measurement.Interface;
using AI_stats_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace AI_stats_measurement.Controllers
{
    [ApiController]
    [Route("api/llm")]
    public class LlmController : ControllerBase
    {
        private readonly IEnumerable<ILlmQuerier> _queriers;

        public LlmController(IEnumerable<ILlmQuerier> queriers)
        {
            _queriers = queriers;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<List<LlmAnswer>>> AskAll([FromBody] Prompt prompt, CancellationToken ct)
        {
            if (prompt is null)
                return BadRequest("Prompt is required.");

            var tasks = _queriers.Select(async q =>
            {
                try
                {
                    var text = await q.AskAsync(prompt, ct);
                    return new ModelResponse(prompt.Id,q.Name, text, null);
                }
                catch (Exception ex)
                {
                    return new ModelResponse(prompt.Id, q.Name, null, ex.Message);
                }
            });

            var results = (await Task.WhenAll(tasks)).ToList();
            return Ok(results);
        }

        public record LlmAnswer(string Provider, string? Answer, string? Error);
    }
}
