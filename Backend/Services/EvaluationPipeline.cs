using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Models.AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Services;
using Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Services
{
    public class EvaluationPipeline
    {
        private readonly LlmAggregator _llmAggregator;
        private readonly FactChecker _checker;
        private readonly AIMeasureDbContext _context;


        public EvaluationPipeline(LlmAggregator llmAggregator, FactChecker checker, AIMeasureDbContext context) { 
            _llmAggregator = llmAggregator;
            _checker = checker;
            _context = context;
        }

        public async Task<List<ExportRow>> RunAsync(List<int> promptIds, CancellationToken ct)
        {
            var prompts = await _context.Prompts
                .Where(p => promptIds.Contains(p.Id))
                .ToListAsync(ct);

            var promptById = prompts.ToDictionary(p => p.Id);

            var rows = new List<ExportRow>();

            var responses = await _llmAggregator.AskByPromptIdsAsync(promptIds, ct);

            foreach (var response in responses)
            {
                if (!promptById.TryGetValue(response.PromptId, out var prompt))
                {
                    continue;
                }

                var parsed = ModelResponseParser.Parse(response.Id, response.RawText);
                var fact = _checker.Check(parsed, prompt.Answer);

                rows.Add(new ExportRow(
                    theme: prompt.Theme,
                    question: prompt.Question,
                    expectedAnswer: prompt.Answer,
                    expectedSource: prompt.Source,
                    actualAnswer: parsed.Answer,
                    actualSource: parsed.Source,
                    provider: response.Provider,
                    rawText: response.RawText,
                    exception: response.Exception,
                    squareMeanRootError: fact.SquareMeanRootError,
                    relativeError: fact.RelativeError,
                    isCorrect: fact.IsCorrect,
                    createdUtc: response.CreatedUtc
                ));
            }

            return rows;
        }
    }
}
