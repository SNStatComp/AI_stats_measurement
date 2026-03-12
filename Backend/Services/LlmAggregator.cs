using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Services
{
    public class LlmAggregator
    {
        private readonly IEnumerable<ILlmQuerier> _queriers;
        private readonly AIMeasureDbContext _context;

        public LlmAggregator(IEnumerable<ILlmQuerier> queriers, AIMeasureDbContext context)
        {
            _queriers = queriers;
            _context = context;
        }

        public async Task<List<ModelResponse>> AskAllAsync(List<Prompt> prompts, CancellationToken ct)
        {
            var tasks = new List<Task<ModelResponse>>();

            foreach (var querier in _queriers)
            {
                foreach (var prompt in prompts)
                {
                    tasks.Add(AskSingleAsync(querier, prompt, ct));
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        private async Task<ModelResponse> AskSingleAsync(ILlmQuerier q,Prompt prompt, CancellationToken ct)
        {
            try
            {
                var text = await q.AskAsync(prompt, ct);
                return new ModelResponse(prompt.Id, q.Name, text, null);
            }
            catch (Exception ex)
            {
                return new ModelResponse(prompt.Id, q.Name, null, ex.Message);
            }
        }

        public async Task<List<ModelResponse>> AskByPromptIdsAsync(List<int> promptIds, CancellationToken ct)
        {
            var tasks = new List<Task<ModelResponse>>();

            var prompts = await _context.Prompts
                .Where(p => promptIds.Contains(p.Id))
                .ToListAsync(ct);

            if (prompts.Count == 0)
                return new List<ModelResponse>();                  

            foreach (var querier in _queriers)
            {
                foreach (var prompt in prompts)
                {
                    tasks.Add(AskSingleAsync(querier, prompt, ct));
                }
            }

            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }
    }
}
