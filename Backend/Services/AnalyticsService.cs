using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Models;

namespace AI_stats_measurement.Backend.Services
{
    public class AnalyticsService
    {
        public AnalyticsService() { }

        public DashboardMetricsDto GetMetrics(
        List<FactCheckResult> results,
        string? nsi,
        string? llm,
        string? theme)
        {
            var filtered = ApplyFilters(results, nsi, llm, theme);

            return new DashboardMetricsDto
            {
                AccuracyScore = ComputeAccuracyScore(filtered),
                ConsistencyScore = ComputeConsistencyScore(filtered),
                FindabilityScore = ComputeFindabilityScore(filtered),
                TotalMeasurements = filtered.Count
            };
        }

        private double ComputeFindabilityScore(List<FactCheckResult> filtered)
        {
            if (filtered.Count == 0) return 0;

            int correctAnswers = filtered.Count(r => r.SourceIsCorrect);

            double ratio = (double)correctAnswers / filtered.Count;

            return ratio * 10;
        }

        private double ComputeConsistencyScore(List<FactCheckResult> filtered)
        {
            if (filtered.Count == 0) return 0;

            var values = filtered
                .Select(r => (double)r.ParsedModelResponse.Answer)
                .ToList();

            double mean = values.Average();

            if (mean == 0) return 0; // avoid division by zero

            double variance = values.Average(v => Math.Pow(v - mean, 2));
            double stdDev = Math.Sqrt(variance);

            double relative = stdDev / mean;

            // Convert to score (inverse relationship)
            double score = (1 - relative) * 10;

            // Clamp between 0 and 10
            return Math.Max(0, Math.Min(10, score));
        }

        public double ComputeAccuracyScore(List<FactCheckResult> factCheckResults)
        {         
            if (!factCheckResults.Any())
                return 0.0;

            double correctAnswersScore = ComputeCorrectAnswerScore(factCheckResults);
            double rmseScore = ComputeRelativeRmseScore(factCheckResults);

            return Math.Round(correctAnswersScore + rmseScore, 2);
        }

        private List<FactCheckResult> ApplyFilters(
            List<FactCheckResult> factCheckResults,
            string? filterByNSI,
            string? filterByLLM,
            string? filterByTheme)
        {
            return factCheckResults.Where(r =>
                (string.IsNullOrWhiteSpace(filterByNSI) ||
                 r.ParsedModelResponse.ModelResponse.Prompt.Provider == filterByNSI) &&
                (string.IsNullOrWhiteSpace(filterByLLM) ||
                 r.ParsedModelResponse.ModelResponse.Provider == filterByLLM) &&
                (string.IsNullOrWhiteSpace(filterByTheme) ||
                 r.ParsedModelResponse.ModelResponse.Prompt.Theme == filterByTheme)
            ).ToList();
        }

        private double ComputeCorrectAnswerScore(List<FactCheckResult> results)
        {
            double total = results.Count;
            double correctCount = results.Count(r => r.AnswerIsCorrect);
            return (correctCount / total) * 6.0;
        }

        private double ComputeRelativeRmseScore(List<FactCheckResult> results)
        {
            double rmse = Math.Sqrt(
                results.Average(r =>
                {
                    double expected = (double)r.ParsedModelResponse.ModelResponse.Prompt.Answer;
                    double actual = (double)r.ParsedModelResponse.Answer;
                    double error = expected - actual;
                    return error * error;
                })
            );

            double meanExpected = results.Average(r =>
                (double)r.ParsedModelResponse.ModelResponse.Prompt.Answer);

            double rrmse = rmse / meanExpected;

            if (rrmse <= 0.05) return 4.0;
            if (rrmse <= 0.10) return 3.0;
            if (rrmse <= 0.20) return 2.0;
            if (rrmse <= 0.30) return 1.0;
            return 0.0;
        }
    }
}
