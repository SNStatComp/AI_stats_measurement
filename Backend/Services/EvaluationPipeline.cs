using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using AI_stats_measurement.Services;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Services
{
    public class EvaluationPipeline
    {
        private readonly LlmAggregator _llmAggregator;
        private readonly FactChecker _checker;
        private readonly SourceNormalizer _sourceNormalizer;
        private readonly AIMeasureDbContext _context;
        private readonly AnalyticsService _analyticsService;

        public EvaluationPipeline(LlmAggregator llmAggregator, FactChecker checker,  AIMeasureDbContext context, SourceNormalizer sourceNormalizer, AnalyticsService analyticsService)
        {
            _llmAggregator = llmAggregator;
            _checker = checker;
            _context = context;
            _sourceNormalizer = sourceNormalizer;
            _analyticsService = analyticsService;
        }

        public async Task<List<ExportRow>> RunAsync(List<int> promptIds, CancellationToken ct)
        {
            // Step 1: Retrieve prompts from the database
            var prompts = await _context.Prompts
                .Include(s => s.Source)
                .Where(p => promptIds.Contains(p.Id))
                .ToListAsync(ct);

            var promptById = prompts.ToDictionary(p => p.Id);

            var rows = new List<ExportRow>();

            // Step 2: Get responses from the LLM aggregator
            var responses = await _llmAggregator.AskByPromptIdsAsync(promptIds, ct);

            _context.ModelResponses.AddRange(responses);
            await _context.SaveChangesAsync(ct);
       
            foreach (var response in responses)
            {             
                if (!promptById.TryGetValue(response.PromptId, out var prompt))
                {
                    continue;
                }

                ParsedModelResponse? parsed = null;


                // Step 3: Parse the model response
                if (response.Prompt.Provider == "CBS")
                {
                    parsed = ModelResponseParser.ParseDutch(response.Id, response.RawText);
                }
                else if (response.Prompt.Provider == "OECD")
                {
                    parsed = ModelResponseParser.ParseEnglish(response.Id, response.RawText);
                }
                else if (response.Prompt.Provider == "StatBank Denmark")
                {
                    parsed = ModelResponseParser.ParseEnglish(response.Id, response.RawText);
                }

                if (parsed is null)
                {
                    // skip
                    throw new InvalidOperationException(
                        $"Unsupported NSI: {response.Prompt.Provider}");
                }

                await _sourceNormalizer.AttachNormalizedSourcesAsync(parsed, ct);

                _context.ParsedModelResponses.Add(parsed);
                await _context.SaveChangesAsync(ct);

                // Step 4: Fact-check the parsed response
                var fact = _checker.Check(parsed, prompt.Answer, "NSI");

                _context.FactCheckResults.Add(fact);
                await _context.SaveChangesAsync(ct);
               
                var actualSources = parsed.ParsedModelResponseSources
                    .Select(p => p.SourceId)
                    .ToList();

                // Step 5: Create export rows
                rows.Add(new ExportRow(
                    theme: prompt.Theme,
                    question: prompt.Question,
                    expectedAnswer: prompt.Answer,
                    expectedSource: prompt.Source.Url,
                    actualAnswer: parsed.Answer,
                    actualSource: actualSources,
                    provider: response.Provider,
                    rawText: response.RawText,
                    exception: response.Exception,
                    squareMeanRootError: 0,
                    relativeError: fact.RelativeError,
                    answerIsCorrect: fact.AnswerIsCorrect,
                    sourceIsCorrect: fact.SourceIsCorrect,
                    createdUtc: response.CreatedUtc
                ));
            }

            _context.ExportRows.AddRange(rows);
            await _context.SaveChangesAsync(ct);

            return rows;
        }
    }
}