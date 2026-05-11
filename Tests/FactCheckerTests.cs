using AI_stats_measurement.Models;
using AI_stats_measurement.Services;
using Xunit;
using static System.Net.Mime.MediaTypeNames;

namespace AI_stats_measurement.Tests
{
    public class FactCheckerTests
    {
        [Fact]
        public void Check_Computes_AbsoluteError()
        {
            var checker = new FactChecker(0.05m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 90,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, 100, "NSI");

            Assert.Equal(10, result.AbsoluteError);
        }

        [Fact]
        public void Check_Computes_RelativeError()
        {
            var checker = new FactChecker(0.20m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 90,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, 100, "NSI");

            Assert.Equal(0.1111111111111111111111111111m, result.RelativeError);
        }

        [Fact]
        public void Check_Marks_AnswerCorrect_WhenWithinTolerance()
        {
            var checker = new FactChecker(0.15m, "NSI website");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 95,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, expectedAnswer: 100, "NSI");

            Assert.True(result.AnswerIsCorrect);
        }

        [Fact]
        public void Check_Marks_AnswerIncorrect_WhenOutsideTolerance()
        {
            var checker = new FactChecker(0.05m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 80,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, expectedAnswer: 100, "NSI");

            Assert.False(result.AnswerIsCorrect);
        }

        [Fact]
        public void Check_Marks_SourceCorrect_WhenExpectedSourceTypeExists()
        {
            var checker = new FactChecker(0.05m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 100,
                extractedSources: new List<ExtractedSource>
                {
                new ExtractedSource
                {
                    Name = "CBS",
                    Url = "https://www.cbs.nl",
                    Type = "NSI database"
                }
                }
            );

            var result = checker.Check(parsed, expectedAnswer: 100, "NSI");

            Assert.True(result.SourceIsCorrect);
        }

        [Fact]
        public void Check_Marks_SourceIncorrect_WhenNoSourcesExist()
        {
            var checker = new FactChecker(0.05m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 100,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, expectedAnswer: 100, "NSI");

            Assert.False(result.SourceIsCorrect);
        }

        [Fact]
        public void Check_Marks_Abstained_WhenAnswerIsZero()
        {
            var checker = new FactChecker(0.05m, "CBS");

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 0,
                extractedSources: new List<ExtractedSource>()
            );

            var result = checker.Check(parsed, expectedAnswer: 100, "NSI");

            Assert.True(result.Abstained);
        }
    }
}