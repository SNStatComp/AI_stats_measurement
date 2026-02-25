using AI_stats_measurement.Models;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Data;

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

            _context.ModelResponses.AddRange(results);
            await _context.SaveChangesAsync(ct);


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

    }
}
