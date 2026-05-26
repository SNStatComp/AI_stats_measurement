using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Backend.Services.Parsing;
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

            var metrics = service.GetMetricsPerNsi(results, new List<string> { "CBS" }, new List<string> { "websearch disabled" }, null );

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

            var metrics = service.GetMetricsPerNsi(results, new List<string> { "CBS" }, null, null);

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
            Assert.Contains(metrics, m => m.Label == "CBS");
            Assert.Contains(metrics, m => m.Label == "OECD");
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

            var metrics = service.GetMetricsPerNsi(results, new List<string> { "CBS" }, null, null);

            Assert.Single(metrics);
            Assert.Equal("CBS", metrics[0].Label);
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

            var weekly = service.GetWeeklyMetricsPerNsi(results, new List<string> { "CBS" }, null, null);

            Assert.True(weekly.ContainsKey("CBS"));
            Assert.Equal(2, weekly["CBS"].Findability.Count);
            Assert.Equal(2, weekly["CBS"].Accuracy.Count);
            Assert.Equal(2, weekly["CBS"].Consistency.Count);
        }

        [Fact]
        public void GetMetricsPerTheme_GroupsResults_PerTheme()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, theme: "Population"),
                CreateResult(2, "CBS", "gpt-4o-mini", 200, 200, true, theme: "Economy"),
            };

            var metrics = service.GetMetricsPerTheme(results, null, null, null);

            Assert.Equal(2, metrics.Count);
            Assert.Contains(metrics, m => m.Label == "Population");
            Assert.Contains(metrics, m => m.Label == "Economy");
        }

        [Fact]
        public void GetMetricsPerModel_GroupsResults_PerModel()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
                CreateResult(2, "CBS", "gemini-2.5-flash-lite", 200, 200, true),
            };

            var metrics = service.GetMetricsPerModel(results, null, null, null);

            Assert.Equal(2, metrics.Count);
            Assert.Contains(metrics, m => m.Label == "gpt-4o-mini");
            Assert.Contains(metrics, m => m.Label == "gemini-2.5-flash-lite");
        }

        [Fact]
        public void GetMetricsPerNsi_ReturnsEmpty_WhenAllResultsAreAbstained()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, abstained: true)
            };

            var metrics = service.GetMetricsPerNsi(results, null, null, null);

            Assert.Empty(metrics);
        }

        [Fact]
        public void GetMetricsPerNsi_Filters_ByTheme()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, theme: "Population"),
                CreateResult(2, "CBS", "gpt-4o-mini", 200, 200, true, theme: "Economy"),
            };

            var metrics = service.GetMetricsPerNsi(results, null, null, new List<string> { "Economy" });

            Assert.Single(metrics);
            Assert.Equal(1, metrics[0].TotalMeasurements);
        }

        [Fact]
        public void GetMetricsPerNsi_Filters_WebsearchEnabled()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-5.4", 100, 100, true),
                CreateResult(2, "CBS", "gpt-4o-mini", 200, 200, true),
            };

            var metrics = service.GetMetricsPerNsi(
                results,
                null,
                new List<string> { "websearch enabled" },
                null
            );

            Assert.Single(metrics);
            Assert.Equal(1, metrics[0].TotalMeasurements);
        }

        [Fact]
        public void GetMetricsPerNsi_Filters_ByFallbackLlmPrefix()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "custom-model-v1", 100, 100, true),
                CreateResult(2, "CBS", "other-model", 200, 200, true),
            };

            var metrics = service.GetMetricsPerNsi(
                results,
                null,
                new List<string> { "custom-model" },
                null
            );

            Assert.Single(metrics);
            Assert.Equal(1, metrics[0].TotalMeasurements);
        }

        [Fact]
        public void GetMetricsPerNsi_ReturnsTopSources()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, sourceUrl: "https://www.cbs.nl/page1"),
                CreateResult(2, "CBS", "gpt-4o-mini", 100, 100, true, sourceUrl: "https://www.cbs.nl/page2"),
                CreateResult(3, "CBS", "gpt-4o-mini", 100, 100, true, sourceUrl: "https://www.oecd.org/page"),
            };

            var metrics = service.GetMetricsPerNsi(results, null, null, null);

            Assert.Single(metrics);
            Assert.Contains(metrics[0].TopSources, s => s.Hostname == "www.cbs.nl" && s.Count == 2);
            Assert.Contains(metrics[0].TopSources, s => s.Hostname == "www.oecd.org" && s.Count == 1);
        }

        [Fact]
        public void GetWeeklyMetricsPerModel_GroupsResults_ByModel()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true),
                CreateResult(2, "CBS", "gemini-2.5-flash-lite", 100, 100, true),
            };

            var weekly = service.GetWeeklyMetricsPerModel(results, null, null, null);

            Assert.True(weekly.ContainsKey("gpt-4o-mini"));
            Assert.True(weekly.ContainsKey("gemini-2.5-flash-lite"));
        }

        [Fact]
        public void GetWeeklyMetricsPerTheme_GroupsResults_ByTheme()
        {
            var service = new AnalyticsService();

            var results = new List<FactCheckResult>
            {
                CreateResult(1, "CBS", "gpt-4o-mini", 100, 100, true, theme: "Population"),
                CreateResult(2, "CBS", "gpt-4o-mini", 100, 100, true, theme: "Economy"),
            };

            var weekly = service.GetWeeklyMetricsPerTheme(results, null, null, null);

            Assert.True(weekly.ContainsKey("Population"));
            Assert.True(weekly.ContainsKey("Economy"));
        }

        private static FactCheckResult CreateResult(int promptId,string nsi,string llm,decimal actualAnswer,decimal expectedAnswer, bool sourceIsCorrect,DateTime? createdUtc = null, string theme = "test",bool abstained = false,string sourceUrl = "https://www.cbs.nl")
        {
            var source = new Source
            {
                Id = promptId,
                Name = "Test Source",
                Url = sourceUrl,
                Type = "Website"
            };

            var prompt = new Prompt(
                nsi,
                "instruction",
                theme,
                DateTime.UtcNow,
                "subject",
                "question",
                expectedAnswer,
                source,
                "answer location"
            );

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

            parsed.Id = promptId;
            parsed.ModelResponse = modelResponse;

            parsed.ParsedModelResponseSources.Add(new ParsedModelResponseSource
            {
                ParsedModelResponseId = parsed.Id,
                ParsedModelResponse = parsed,
                SourceId = source.Id,
                Source = source
            });

            var absoluteError = Math.Abs(expectedAnswer - actualAnswer);
            var relativeError = expectedAnswer == 0 ? 0 : absoluteError / expectedAnswer;

            var fact = new FactCheckResult(
                parsedModelResponseId: parsed.Id,
                absoluteError: absoluteError,
                relativeError: relativeError,
                answerIsCorrect: actualAnswer == expectedAnswer,
                sourceIsCorrect: sourceIsCorrect,
                abstained: abstained
            );

            fact.ParsedModelResponse = parsed;

            return fact;
        }
    }
}

