using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Models.AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Services;
using Azure;
using Elfie.Serialization;
using Google.GenAI.Types;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;

namespace AI_stats_measurement.Backend.Services
{
    public class EvaluationPipeline
    {
        private readonly LlmAggregator _llmAggregator;
        private readonly FactChecker _checker;
        private readonly AIMeasureDbContext _context;

        public EvaluationPipeline(LlmAggregator llmAggregator, FactChecker checker, AIMeasureDbContext context)
        {
            _llmAggregator = llmAggregator;
            _checker = checker;
            _context = context;
        }

        public async Task<List<ExportRow>> RunAsync(List<int> promptIds, CancellationToken ct)
        {
            // Step 1: Retrieve prompts from the database
            var prompts = await _context.Prompts
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

                // Step 3: Parse the model response
                var parsed = ModelResponseParser.Parse(response.Id, response.RawText);

                _context.ParsedModelResponses.Add(parsed);
                await _context.SaveChangesAsync(ct);

                // Retrieve the last 3 parsed responses for the same prompt to use as context for cosistency
                var previousParsed = _context.ParsedModelResponses
                .Include(r => r.ModelResponse)
                .Where(r => r.ModelResponse.PromptId == parsed.ModelResponse.PromptId
                && r.ModelResponse.Provider == parsed.ModelResponse.Provider
                )
                .OrderByDescending(r => r.Id)
                .Take(3)
                .ToList();

                // Step 4: Fact-check the parsed response
                var fact = _checker.Check(previousParsed, parsed, prompt.Answer, prompt.Source);

                _context.FactCheckResults.Add(fact);
                await _context.SaveChangesAsync(ct);

                // Step 5: Create export rows
                rows.Add(new ExportRow(
                    theme: prompt.Theme,
                    question: prompt.Question,
                    expectedAnswer: prompt.Answer,
                    expectedSource: prompt.Source,
                    actualAnswer: parsed.Answer,
                    actualSource: parsed.Sources,
                    provider: response.Provider,
                    rawText: response.RawText,
                    exception: response.Exception,
                    squareMeanRootError: fact.SquareMeanRootError,
                    relativeError: fact.RelativeError,
                    answerIsCorrect: fact.AnswerIsCorrect,
                    sourceIsCorrect: fact.SourceIsCorrect,
                    averageRelativeError: fact.AverageRelativeError,
                    averageAnswer: fact.AverageAnswer,
                    averageAnswerCorrectness: fact.AverageAnswerCorrectness,
                    averageSourceCorrectness: fact.AverageSourceCorrectness,
                    createdUtc: response.CreatedUtc
                ));
            }

            _context.ExportRows.AddRange(rows);
            await _context.SaveChangesAsync(ct);

            return rows;
        }             
    }
}