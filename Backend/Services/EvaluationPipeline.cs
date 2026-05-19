using AI_stats_measurement.Backend.Interface;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services.Parsing;
using AI_stats_measurement.Data;
using AI_stats_measurement.Services;
using Azure;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Services
{
    public class EvaluationPipeline
    {
        private readonly ILlmAggregator _llmAggregator;
        private readonly FactChecker _checker;
        private readonly SourceNormalizer _sourceNormalizer;
        private readonly AIMeasureDbContext _context;

        public EvaluationPipeline(ILlmAggregator llmAggregator, FactChecker checker,  AIMeasureDbContext context, SourceNormalizer sourceNormalizer)
        {
            _llmAggregator = llmAggregator;
            _checker = checker;
            _context = context;
            _sourceNormalizer = sourceNormalizer;
        }

        public async Task<List<ExportRow>> RunAsync(List<int> promptIds, List<string> modelNames , Guid jobId, CancellationToken ct)
        {
            // Step 1: Retrieve prompts from the database
            var prompts = await _context.Prompts
                .Include(s => s.Source)
                .Where(p => promptIds.Contains(p.Id))
                .ToListAsync(ct);

            var promptById = prompts.ToDictionary(p => p.Id);

            var rows = new List<ExportRow>();

            // Step 2: Get responses from the LLM aggregator
            var responses = await _llmAggregator.AskByPromptIdsAsync(promptIds, modelNames, jobId, ct);

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
                parsed = prompt.Provider switch
                {
                    "CBS" => ModelResponseParser.ParseDutch(response.Id, response.RawText),
                    "OECD" => ModelResponseParser.ParseEnglish(response.Id, response.RawText),
                    "StatBank Denmark" => ModelResponseParser.ParseEnglish(response.Id, response.RawText),
                    _ => throw new InvalidOperationException($"Unsupported NSI: {prompt.Provider}")
                };

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
                    modelResponseId: response.Id,
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

            var providerOrder = new[]
            {
                "gpt",
                "gemini",
                "grok"
            };

            return rows = rows
                .OrderBy(r => r.Question)
                .ThenBy(r => Array.IndexOf(providerOrder, r.Provider))
                .ToList();
        }

        public async Task<List<ExportRow>> RecalculateAsync(CancellationToken ct)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            var prompts = await _context.Prompts
                .Include(p => p.Source)
                .ToListAsync(ct);

            var promptById = prompts.ToDictionary(p => p.Id);

            // remove old parsed responses and export rows for the given promptIds
            var oldParsedResponses = await _context.ParsedModelResponses
                .Include(p => p.ModelResponse)
                .ToListAsync(ct);

            _context.ParsedModelResponses.RemoveRange(oldParsedResponses);

            var ExportRows = await _context.ExportRows
                .ToListAsync(ct);

            _context.ExportRows.RemoveRange(ExportRows);
            await _context.SaveChangesAsync(ct);

            var usedSourceIds = await _context.Prompts
                .Select(p => p.SourceId)
                .Distinct()
                .ToListAsync(ct);

            var unusedSources = await _context.Sources
                .Where(s => !usedSourceIds.Contains(s.Id))
                .ToListAsync(ct);

            _context.Sources.RemoveRange(unusedSources);

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);


            // retrieve model responses for the given promptIds
            var responses = await _context.ModelResponses
                .Include(r => r.Prompt)
                .OrderBy(r => r.PromptId)
                .ThenBy(r => r.CreatedUtc)
                .ToListAsync(ct);

            var rows = new List<ExportRow>();


            foreach (var response in responses)
            {
                ct.ThrowIfCancellationRequested();

                if (!promptById.TryGetValue(response.PromptId, out var prompt))
                {
                    continue;
                }

                ParsedModelResponse? parsed = prompt.Provider switch
                {
                    "CBS" => ModelResponseParser.ParseDutch(response.Id, response.RawText),
                    "OECD" => ModelResponseParser.ParseEnglish(response.Id, response.RawText),
                    "StatBank Denmark" => ModelResponseParser.ParseEnglish(response.Id, response.RawText),
                    _ => throw new InvalidOperationException($"Unsupported NSI: {prompt.Provider}")
                };

                if (parsed is null)
                {
                    throw new InvalidOperationException(
                        $"Parser returned null for ModelResponse {response.Id} ({prompt.Provider}).");
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
                    modelResponseId: response.Id,
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

            var providerOrder = new[]
            {
                "gpt",
                "gemini",
                "grok"
            };

            return rows = rows
                .OrderBy(r => r.Question)
                .ThenBy(r => Array.IndexOf(providerOrder, r.Provider))
                .ToList();
        }
    }
}