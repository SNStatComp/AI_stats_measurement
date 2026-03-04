using AI_stats_measurement.Models;

namespace AI_stats_measurement.Services
{
    public class FactChecker
    {
        public decimal RelativeTolerance { get; }

        public FactChecker(decimal relativeTolerance)
        {
            RelativeTolerance = relativeTolerance;
        }

        public FactCheckResult Check(ParsedModelResponse parsed, decimal actual)
        {
            if (parsed is null)
                throw new ArgumentNullException(nameof(parsed));

            decimal predicted = parsed.Answer;

            decimal rmse = ComputeRmse(actual, predicted);
            decimal relativeError = ComputeRelativeError(actual, predicted);

            bool isCorrect = relativeError <= RelativeTolerance;

            return new FactCheckResult(
                parsed.Id,
                rmse,
                relativeError,
                isCorrect
            );
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

        private static bool IsWithinRelativeTolerance(decimal actual, decimal predicted, decimal tolRel)
        {
            // If actual is 0, relative tolerance is undefined
            if (actual == 0m) return predicted == 0m;

            decimal relError = Math.Abs(predicted - actual) / Math.Abs(actual);
            return relError <= tolRel;
        }
    }
}
