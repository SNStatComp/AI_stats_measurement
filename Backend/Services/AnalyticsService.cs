using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Models;
using System.Collections.Generic;

namespace AI_stats_measurement.Backend.Services
{
    public class AnalyticsService
    {
        public AnalyticsService() { }

        public List<DashboardMetricsByNsiDto> GetMetricsPerNsi(List<FactCheckResult> results, string? nsi, string? llm, string? theme)
        {
            var filtered = ApplyFilters(results, null, llm, theme);

            if (!string.IsNullOrWhiteSpace(nsi))
            {
                var nsiResults = filtered
                    .Where(r => r.ParsedModelResponse.ModelResponse.Prompt.Provider == nsi)
                    .ToList();

                var sources = GetMostCitedSources(nsiResults);

                return new List<DashboardMetricsByNsiDto>
        {
            MapToMetricsDto(nsi, nsiResults, sources)
        };
            }

            return filtered
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Prompt.Provider)
                .Select(group =>
                {
                    var groupResults = group.ToList();
                    var sources = GetMostCitedSources(groupResults);

                    return MapToMetricsDto(group.Key, groupResults, sources);
                })
                .ToList();
        }

        private DashboardMetricsByNsiDto MapToMetricsDto(string nsi, List<FactCheckResult> results, List<SourceCount> sources)
        {
            return new DashboardMetricsByNsiDto
            {
                Nsi = nsi,
                AccuracyScore = ComputeAccuracyScore(results),
                ConsistencyScore = ComputeConsistencyScore(results),
                FindabilityScore = ComputeFindabilityScore(results),
                TotalMeasurements = results.Count,
                TopSources = sources
            };
        }

        private double ComputeFindabilityScore(List<FactCheckResult> filtered)
        {
            if (filtered.Count == 0) return 0;

            int correctAnswers = filtered.Count(r => r.SourceIsCorrect);
            double ratio = (double)correctAnswers / filtered.Count;

            return ratio * 10;
        }

        private double ComputeRelativeMad(List<FactCheckResult> promptResults)
        {
            var values = promptResults
                .Select(r => (double)r.ParsedModelResponse.Answer)
                .OrderBy(v => v)
                .ToList();

            if (values.Count < 2)
                return 0;

            double median = GetMedian(values);

            var deviations = values
                .Select(v => Math.Abs(v - median))
                .ToList();

            double mad = deviations.Average(); 
            double scale = Math.Max(Math.Abs(median), 1.0);

            return mad / scale;
        }

        private double GetMedian(List<double> values)
        {
            int n = values.Count;

            if (n % 2 == 1)
                return values[n / 2];

            return (values[n / 2 - 1] + values[n / 2]) / 2.0;
        }

        private double ComputeConsistencyScore(List<FactCheckResult> results)
        {
            // 
            var perPrompt = results
                .Where(r => !r.Abstained && r.RelativeError != 1)
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Prompt.Id)
                .Select(g => ComputeRelativeMad(g.ToList()))
                .ToList();

            if (perPrompt.Count == 0) return 0;

            double avgRelativeMad = perPrompt.Average();

            double score = (1 - avgRelativeMad) * 10;
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

        private List<FactCheckResult> ApplyFilters(List<FactCheckResult> factCheckResults, string? filterByNSI, string? filterByLLM, string? filterByTheme)
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
            if (results.Count == 0) return 0;

            double total = results.Count;
            double correctCount = results.Count(r => r.AnswerIsCorrect);
            return (correctCount / total) * 6.0;
        }

        private double ComputeRelativeRmseScore(List<FactCheckResult> results)
        {
            if (results.Count == 0) return 0;

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

            if (meanExpected == 0) return 0;

            double rrmse = rmse / meanExpected;

            if (rrmse <= 0.05) return 4.0;
            if (rrmse <= 0.10) return 3.0;
            if (rrmse <= 0.25) return 2.0;
            if (rrmse <= 0.50) return 1.0;
            return 0.0;
        }
        private List<SourceCount> GetMostCitedSources(List<FactCheckResult> results)
        {
            return results
                .Where(r => r.ParsedModelResponse != null)
                .SelectMany(r => r.ParsedModelResponse.ParsedModelResponseSources ?? [])
                .Where(ps => ps.Source != null && !string.IsNullOrWhiteSpace(ps.Source.Url))
                .Select(ps => new Uri(ps.Source.Url).Host)
                .GroupBy(host => host)
                .Select(g => new SourceCount
                {
                    Hostname = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();
        }
    }
}