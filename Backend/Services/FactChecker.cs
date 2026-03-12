using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using AI_stats_measurement.Models;

namespace AI_stats_measurement.Services
{
    public class FactChecker
    {
        public decimal RelativeTolerance { get; }
        public string Provider { get; }

        public FactChecker(decimal relativeTolerance, string provider)
        {
            RelativeTolerance = relativeTolerance;
            Provider = provider;
        }

        public FactCheckResult Check(List<ParsedModelResponse> previousParsed, ParsedModelResponse parsed, decimal expectedAnswer, string expectedSource)
        {
            if (parsed is null)
                throw new ArgumentNullException(nameof(parsed));

            previousParsed ??= new List<ParsedModelResponse>();

            decimal actualAnswer = parsed.Answer;

            decimal rmse = ComputeRmse(expectedAnswer, actualAnswer);
            decimal relativeError = ComputeRelativeError(expectedAnswer, actualAnswer);

            bool answerIsCorrect = relativeError <= RelativeTolerance;
            bool sourceIsCorrect = ComputeSourceCorrectness(parsed.Sources, expectedSource);

            decimal averageRelativeError = ComputeAverageRelativeError(previousParsed, expectedAnswer);
            decimal averageAnswer = ComputeConsistencyAnswer(previousParsed.Select(p => p.Answer).ToList());
            decimal averageAnswerCorrectness = ComputeAverageAnswerCorrectness(previousParsed, expectedAnswer);
            decimal averageSourceCorrectness = ComputeAverageSourceCorrectness(previousParsed, expectedSource);

            var result = new FactCheckResult(
                parsed.Id,
                rmse,
                relativeError,
                answerIsCorrect,
                sourceIsCorrect,
                averageRelativeError,
                averageAnswer,
                averageAnswerCorrectness,
                averageSourceCorrectness
            );

            return result;
        }

        private static decimal ComputeRmse(decimal actual, decimal predicted)
        {
            decimal diff = predicted - actual;
            return Math.Abs(diff);
        }

        private static decimal ComputeRelativeError(decimal actual, decimal predicted)
        {
            if (actual == 0m)
                return predicted == 0m ? 0m : 1m;

            return Math.Abs(predicted - actual) / Math.Abs(actual);
        }

        public static decimal ComputeConsistencyAnswer(List<decimal> parsedModelResponses)
        {
            if (parsedModelResponses is null || parsedModelResponses.Count == 0)
                return 0m;

            return parsedModelResponses.Average();
        }

        private decimal ComputeAverageRelativeError(List<ParsedModelResponse> previousParsed, decimal actual)
        {
            if (previousParsed is null || previousParsed.Count == 0)
                return 0m;

            return previousParsed
                .Select(p => ComputeRelativeError(actual, p.Answer))
                .Average();
        }

        private decimal ComputeAverageAnswerCorrectness(List<ParsedModelResponse> previousParsed, decimal actual)
        {
            if (previousParsed is null || previousParsed.Count == 0)
                return 0m;

            return previousParsed
                .Average(p => ComputeRelativeError(actual, p.Answer) <= RelativeTolerance ? 1m : 0m);
        }

        private decimal ComputeAverageSourceCorrectness(List<ParsedModelResponse> previousParsed, string expectedSource)
        {
            if (previousParsed is null || previousParsed.Count == 0)
                return 0m;

            return previousParsed
                .Average(p => ComputeSourceCorrectness(p.Sources, expectedSource) ? 1m : 0m);
        }

        private static bool ComputeSourceCorrectness(List<string>? actualSources, string expectedSource)
        {
            if (actualSources is null || actualSources.Count == 0)
                return false;

            return actualSources.Any(s =>
                !string.IsNullOrWhiteSpace(s) &&
                s.Contains("cbs", StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeSource(string source)
        {
            return source
                .Trim()
                .TrimEnd('/')
                .ToLowerInvariant();
        }
    }
}
