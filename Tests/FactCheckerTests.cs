using AI_stats_measurement.Models;
using AI_stats_measurement.Services;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace AI_stats_measurement.Tests
{
    public class FactCheckerTests
    {
        

        [Fact]
        public void ComputeConsistencyAnswer_ReturnsAverage()
        {
            var answers = new List<decimal> { 400000m, 500000m, 600000m };

            var result = FactChecker.ComputeConsistencyAnswer(answers);

            Assert.Equal(500000m, result);
        }

        [Fact]
        public void ComputeConsistencyAnswer_ReturnsZero_WhenEmpty()
        {
            var answers = new List<decimal>();

            var result = FactChecker.ComputeConsistencyAnswer(answers);

            Assert.Equal(0m, result);
        }

        [Fact]
        public void Check_ComputesRelativeError_AndAnswerCorrectness()
        {
            var factChecker = new FactChecker(0.05m, "CBS");

            var previousParsed = new List<ParsedModelResponse>();

            var parsed = new ParsedModelResponse(
                1,
                105m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.Equal(5m, result.SquareMeanRootError);
            Assert.Equal(0.05m, result.RelativeError);
            Assert.True(result.AnswerIsCorrect);
        }

        [Fact]
        public void Check_ComputesSourceCorrectness_True_WhenExpectedSourceMatches()
        {
            var factChecker = new FactChecker(0.05m, "cbs");

            var previousParsed = new List<ParsedModelResponse>();

            var parsed = new ParsedModelResponse(
                1,
                100m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.True(result.SourceIsCorrect);
        }

        [Fact]
        public void Check_ComputesAverageRelativeError_FromPreviousParsed()
        {
            var factChecker = new FactChecker(0.05m, "cbs");

            var previousParsed = new List<ParsedModelResponse>
            {
                new ParsedModelResponse(1, 90m, new List<string> { "https://www.cbs.nl" }),
                new ParsedModelResponse(2, 110m, new List<string> { "https://www.cbs.nl" })
            };

            var parsed = new ParsedModelResponse(
                3,
                100m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.Equal(0.10m, result.AverageRelativeError);
        }

        [Fact]
        public void Check_ComputesAverageAnswer_FromPreviousParsed()
        {
            var factChecker = new FactChecker(0.05m, "cbs");

            var previousParsed = new List<ParsedModelResponse>
            {
                new ParsedModelResponse(1, 80m, new List<string> { "https://www.cbs.nl" }),
                new ParsedModelResponse(2, 100m, new List<string> { "https://www.cbs.nl" }),
                new ParsedModelResponse(3, 120m, new List<string> { "https://www.cbs.nl" })
            };

            var parsed = new ParsedModelResponse(
                4,
                100m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.Equal(100m, result.AverageAnswer);
        }

        [Fact]
        public void Check_ComputesAverageAnswerCorrectness()
        {
            var factChecker = new FactChecker(0.05m, "cbs");

            var previousParsed = new List<ParsedModelResponse>
            {
                new ParsedModelResponse(1, 100m, new List<string> { "https://www.cbs.nl" }), // correct
                new ParsedModelResponse(2, 104m, new List<string> { "https://www.cbs.nl" }), // correct
                new ParsedModelResponse(3, 130m, new List<string> { "https://www.cbs.nl" })  // incorrect
            };

            var parsed = new ParsedModelResponse(
                4,
                100m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.Equal(2m / 3m, result.AverageAnswerCorrectness);
        }

        [Fact]
        public void Check_ComputesAverageSourceCorrectness()
        {
            var factChecker = new FactChecker(0.05m, "cbs");

            var previousParsed = new List<ParsedModelResponse>
            {
                new ParsedModelResponse(1, 100m, new List<string> { "https://www.cbs.nl" }),
                new ParsedModelResponse(2, 100m, new List<string> { "https://www.cbs.nl" }),
                new ParsedModelResponse(3, 100m, new List<string> { "https://www.example.com" })
            };

            var parsed = new ParsedModelResponse(
                4,
                100m,
                new List<string> { "https://www.cbs.nl" }
            );

            var result = factChecker.Check(
                previousParsed,
                parsed,
                100m,
                "https://www.cbs.nl"
            );

            Assert.Equal(2m / 3m, result.AverageSourceCorrectness);
        }
    }
}
