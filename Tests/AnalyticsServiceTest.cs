using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Backend.Services.Parsing;
using AI_stats_measurement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_stats_measurement.Tests
{

    public class AnalyticsServiceTest
    {
        [Fact]
        public void GetMetricsPerNsi_Computes_FindabilityScore()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
        {
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
            CreateResult(2, "CBS", "gpt-4o-mini", 90, 100, false),
        };

            var metrics = service.GetMetricsPerNsi(results, "CBS", null, null);

            Assert.Single(metrics);
            Assert.Equal(5.0, metrics[0].FindabilityScore);
        }

        [Fact]
        public void GetMetricsPerNsi_Computes_PerfectConsistencyScore_WhenAnswersAreEqual()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
        {
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
        };

            var metrics = service.GetMetricsPerNsi(results, "CBS", null, null);

            Assert.Single(metrics);
            Assert.Equal(10.0, metrics[0].ConsistencyScore);
        }

        [Fact]
        public void GetMetricsPerNsi_GroupsResults_PerNsi()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
        {
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
            CreateResult(2, "OECD", "gpt-4o-mini", 200, 200, true),
        };

            var metrics = service.GetMetricsPerNsi(results, null, null, null);

            Assert.Equal(2, metrics.Count);
            Assert.Contains(metrics, m => m.Nsi == "CBS");
            Assert.Contains(metrics, m => m.Nsi == "OECD");
        }

        [Fact]
        public void GetMetricsPerNsi_Filters_ByNsi()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
        {
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
            CreateResult(2, "OECD", "gpt-4o-mini", 200, 200, true),
        };

            var metrics = service.GetMetricsPerNsi(results, "CBS", null, null);

            Assert.Single(metrics);
            Assert.Equal("CBS", metrics[0].Nsi);
        }

       
        [Fact]
        public void GetWeeklyMetricsPerNsi_GroupsResults_ByWeek()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
        {
            CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)),
            CreateResult(2, "CBS", "gpt-4o-mini", 90, 100, false, new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc)),
        };

            var weekly = service.GetWeeklyMetricsPerNsi(results, "CBS", null, null);

            Assert.True(weekly.ContainsKey("CBS"));
            Assert.Equal(2, weekly["CBS"].Findability.Count);
            Assert.Equal(2, weekly["CBS"].Accuracy.Count);
            Assert.Equal(2, weekly["CBS"].Consistency.Count);
        }

        private static FactCheckResult CreateResult(
            int promptId,
            string nsi,
            string llm,
            decimal actualAnswer,
            decimal expectedAnswer,
            bool sourceIsCorrect,
            DateTime? createdUtc = null)
        {
            Source source = new Source { Id = 1, Name = "Test Source" };
            var prompt = new Prompt(nsi, "test", "test", DateTime.Now, "none", "question", expectedAnswer, source, "");

            var modelResponse = ModelResponse.Import(
                id: promptId,
                promptId: promptId,
                provider: llm,
                rawText: "test response",
                exception: null,
                createdUtc: createdUtc ?? DateTime.UtcNow
            );

            modelResponse.Prompt = prompt;

            var parsed = new ParsedModelResponse(
                modelResponseId: modelResponse.Id,
                answer: actualAnswer,
                extractedSources: new List<ExtractedSource>()
            );

            parsed.ModelResponse = modelResponse;

            var absoluteError = Math.Abs(expectedAnswer - actualAnswer);
            var relativeError = expectedAnswer == 0 ? 0 : absoluteError / expectedAnswer;

            var fact = new FactCheckResult(
                parsedModelResponseId: parsed.Id,
                absoluteError: absoluteError,
                relativeError: relativeError,
                answerIsCorrect: actualAnswer == expectedAnswer,
                sourceIsCorrect: sourceIsCorrect,
                abstained: false
            );

            fact.ParsedModelResponse = parsed;

            return fact;
        }    
    }
}

