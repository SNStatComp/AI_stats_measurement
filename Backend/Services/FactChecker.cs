using AI_stats_measurement.Backend.Models;
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

        public FactCheckResult Check(ParsedModelResponse parsed, decimal expectedAnswer, string expectedSource)
        {
            if (parsed is null)
                throw new ArgumentNullException(nameof(parsed));

            decimal actualAnswer = parsed.Answer;

            decimal absoluteError = AbsoluteError(expectedAnswer,actualAnswer);
            decimal relativeError = ComputeRelativeError(actualAnswer, expectedAnswer);

            bool answerIsCorrect = relativeError <= RelativeTolerance;
            bool sourceIsCorrect = ComputeSourceCorrectness(expectedSource, parsed.ExtractedSources);

            return new FactCheckResult(
                parsed.Id,
                absoluteError,
                relativeError,
                answerIsCorrect,
                sourceIsCorrect
            );
        }

        private static decimal ComputeRelativeError(decimal actual, decimal predicted)
        {
            if (actual == 0m)
                return predicted == 0m ? 0m : 1m;

            return Math.Abs(predicted - actual) / Math.Abs(actual);
        }

        private static bool ComputeSourceCorrectness(string expectedSource, List<ExtractedSource>? actualSources)
        {
            if (actualSources is null || actualSources.Count == 0)
                return false;

            return actualSources.Any(s =>
                s != null &&
                (
                    (!string.IsNullOrWhiteSpace(s.Name) &&
                     s.Name.Contains(expectedSource, StringComparison.OrdinalIgnoreCase))
                    ||
                    (!string.IsNullOrWhiteSpace(s.Url) &&
                     s.Url.Contains(expectedSource, StringComparison.OrdinalIgnoreCase))
                )
            );
        }

        private static decimal AbsoluteError(decimal actual, decimal predicted)
        {
            return Math.Abs(predicted - actual);
        }
    }
}
