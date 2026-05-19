using AI_stats_measurement.Backend.Interface;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Services
{
    public class LlmAggregator : ILlmAggregator
    {
        private readonly IEnumerable<ILlmQuerier> _queriers;
        private readonly AIMeasureDbContext _context;

        public LlmAggregator(IEnumerable<ILlmQuerier> queriers, AIMeasureDbContext context)
        {
            _queriers = queriers;
            _context = context;
        }

        private async Task<ModelResponse> AskSingleAsync(ILlmQuerier q, Prompt prompt, Guid jobId, CancellationToken ct) 
        { 
            try
            {
                var text = await q.AskAsync(prompt, ct);
                return new ModelResponse(prompt.Id, q.Name, text, null, jobId);
            }
            catch (Exception ex)
            {
                return new ModelResponse(prompt.Id, q.Name, null, ex.Message, jobId);
            }
        }

        public virtual async Task<List<ModelResponse>> AskByPromptIdsAsync(List<int> promptIds, List<string> modelNames, Guid jobId, CancellationToken ct)
        {
            var tasks = new List<Task<ModelResponse>>();

            var prompts = await _context.Prompts
                .Where(p => promptIds.Contains(p.Id))
                .ToListAsync(ct);

            if (prompts.Count == 0)
                return new List<ModelResponse>();                  

            foreach (var querier in _queriers)
            {
                if (!modelNames.Contains(querier.Name))
                    {
                    continue;
                    }
                               
                foreach (var prompt in prompts)
                {
                    tasks.Add(AskSingleAsync(querier, prompt, jobId, ct));
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}
