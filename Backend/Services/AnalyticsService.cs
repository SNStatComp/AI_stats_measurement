using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;
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

                return new List<DashboardMetricsByNsiDto>{MapToMetricsDto(nsi, nsiResults, sources)};
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

        public List<DashboardMetricsByNsiDto> GetMetricsPerModel(List<FactCheckResult> results,string? nsi, string? llm, string? theme)
        {
            var filtered = ApplyFilters(results, nsi, llm, theme);

            return filtered
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Provider)
                .Select(group =>
                {
                    var groupResults = group.ToList();
                    var sources = GetMostCitedSources(groupResults);

                    return MapToMetricsDto(group.Key, groupResults, sources);
                })
                .ToList();
        }

        public List<DashboardMetricsByNsiDto> GetMetricsPerTheme(List<FactCheckResult> results, string? nsi, string? llm, string? theme)
            {
            var filtered = ApplyFilters(results, nsi, llm, theme);

            return filtered
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Prompt.Theme)
                .Select(group =>
                {
                    var groupResults = group.ToList();
                    var sources = GetMostCitedSources(groupResults);

                    return MapToMetricsDto(group.Key, groupResults, sources);
                })
                .ToList();
        }

        private DashboardMetricsByNsiDto MapToMetricsDto(string label, List<FactCheckResult> results, List<SourceCount> sources)
        {
            var accuracy = ComputeAccuracyMetric(results);
            var consistency = ComputeConsistencyMetric(results);
            var findability = ComputeFindabilityMetric(results);

            return new DashboardMetricsByNsiDto
            {
                Label = label,
                AccuracyScore = accuracy.Score,
                ConsistencyScore = consistency.Score,
                FindabilityScore = findability.Score,
                AccuracyScoreTooltip = accuracy.Tooltip,
                ConsistencyScoreTooltip = consistency.Tooltip,
                FindabilityScoreTooltip = findability.Tooltip,
                TotalMeasurements = results.Count,
                TopSources = sources
            };
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

            double mad = GetMedian(deviations.OrderBy(d => d).ToList());
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

        private MetricResultDto ComputeConsistencyMetric(List<FactCheckResult> results)
        {
            var perPrompt = results
                .Where(r => !r.Abstained)
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Prompt.Id)
                .Select(g => ComputeRelativeMad(g.ToList()))
                .ToList();

            if (perPrompt.Count == 0)
            {
                return new MetricResultDto
                {
                    Score = 0,
                    Tooltip = "Consistency score: no valid repeated prompt results were available."
                };
            }

            double avgRelativeMad = perPrompt.Average();
            double score = 10 - (22.5 * avgRelativeMad);
            score = Math.Max(0, Math.Min(10, score));

            return new MetricResultDto
            {
                Score = Math.Round(score, 2),
                Tooltip =
                    $"Measures how consistent the LLM answers are across repeated runs of the same prompt.\n\n" +

                    $"Step 1: For each prompt, the relative MAD (Median Absolute Deviation) is calculated to measure variation between answers.\n" +
                    $"Step 2: The average relative MAD across all prompts is computed.\n\n" +

                    $"Calculation:\n" +
                    $"score = 10 - (22.5 × average relative MAD)\n\n" +

                    $"average relative MAD = {avgRelativeMad:F4} (~{avgRelativeMad * 100:F1}% deviation)\n" +
                    $"final score = {score:F2} (scale: 0–10, where 10 = perfectly consistent)"
            };
        }

        private MetricResultDto ComputeAccuracyMetric(List<FactCheckResult> factCheckResults)
        {
            if (!factCheckResults.Any())
            {
                return new MetricResultDto
                {
                    Score = 0,
                    Tooltip = "Accuracy score: no results were available."
                };
            }

            var perPromptRrmse = factCheckResults
                .Where(r => !r.Abstained)
                .GroupBy(r => r.ParsedModelResponse.ModelResponse.Prompt.Id)
                .Select(g => ComputeRelativeRmsePerPrompt(g.ToList()))
                .OrderBy(x => x)
                .ToList();

            if (perPromptRrmse.Count == 0)
            {
                return new MetricResultDto
                {
                    Score = 0,
                    Tooltip = "Accuracy score: no valid prompt results were available."
                };
            }

            double medianRelativeRmse = GetMedian(perPromptRrmse);

            double score = 10 - (15 * medianRelativeRmse);
            score = Math.Max(0, Math.Min(10, score));

            return new MetricResultDto
            {
                Score = Math.Round(score, 2),
                Tooltip =
                    $"Measures how close the model’s answers are to the expected values across repeated runs of the same prompt.\n\n" +
                    $"Calculation:\n" +
                    $"Step 1: For each prompt, the relative RMSE is calculated.\n" +
                    $"RMSE = sqrt(mean((expected - actual)^2))\n" +
                    $"relative RMSE = RMSE / median(expected)\n" +
                    $"Step 2: The median of all per-prompt relative RMSE values is used.\n" +
                    $"Step 3: The final score is calculated as:\n" +
                    $"score = 10 - (15 × median relative RMSE)\n\n" +
                    $"median relative RMSE = {medianRelativeRmse:F4} (~{medianRelativeRmse * 100:F1}% error)\n" +
                    $"final score = {score:F2} (scale: 0–10, where 10 = perfect accuracy)"
            };
        }

        private double ComputeRelativeRmsePerPrompt(List<FactCheckResult> promptResults)
        {
            if (promptResults.Count == 0)
                return 0;

            double rmse = Math.Sqrt(
                promptResults.Average(r =>
                {
                    double expected = (double)r.ParsedModelResponse.ModelResponse.Prompt.Answer;
                    double actual = (double)r.ParsedModelResponse.Answer;
                    double error = expected - actual;
                    return error * error;
                })
            );

            var expectedValues = promptResults
                .Select(r => (double)r.ParsedModelResponse.ModelResponse.Prompt.Answer)
                .OrderBy(x => x)
                .ToList();

            double medianExpected = GetMedian(expectedValues);
            double scale = Math.Max(Math.Abs(medianExpected), 1.0);

            double rrmse = rmse / scale;

            // Cap the relative RMSE at 1.0 to prevent extreme outliers from skewing the consistency score too much
            rrmse = Math.Min(rrmse, 1.0);

            return rrmse;
        }

        private MetricResultDto ComputeFindabilityMetric(List<FactCheckResult> filtered)
        {
            if (filtered.Count == 0)
            {
                return new MetricResultDto
                {
                    Score = 0,
                    Tooltip = "Findability score: no results were available."
                };
            }

            int correctSources = filtered.Count(r => r.SourceIsCorrect);
            double ratio = (double)correctSources / filtered.Count;
            double score = ratio * 10;

            return new MetricResultDto
            {
                Score = Math.Round(score, 2),
                Tooltip =
                    $"This score shows how often the model cited a correct source. " +
                    $"Calculation: findability score = (correctly cited sources / total results) × 10. " +
                    $"Correct sources = {correctSources} out of {filtered.Count}. " +
                    $"Final findability score = {score:F2}."
            };
        }

        private List<FactCheckResult> ApplyFilters(List<FactCheckResult> factCheckResults, string? filterByNSI, string? filterByLLM, string? filterByTheme)
        {
            return factCheckResults.Where(r =>
                !r.Abstained &&
                (string.IsNullOrWhiteSpace(filterByNSI) ||
                 r.ParsedModelResponse.ModelResponse.Prompt.Provider == filterByNSI) &&
                MatchesLlmGroup(r.ParsedModelResponse.ModelResponse.Provider, filterByLLM) &&
                (string.IsNullOrWhiteSpace(filterByTheme) ||
                 r.ParsedModelResponse.ModelResponse.Prompt.Theme == filterByTheme)
            ).ToList();
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

        private List<SourceCount> GetMostCitedDatasets(List<FactCheckResult> results)
        {
            return results
                .Where(r => r.ParsedModelResponse != null)
                .SelectMany(r => r.ParsedModelResponse.ParsedModelResponseSources ?? [])
                .Where(ps => ps.Source != null && !string.IsNullOrWhiteSpace(ps.Source.Url))
                .Select(ps => ExtractDatasetKey(ps.Source.Url!))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(dataset => dataset)
                .Select(g => new SourceCount
                {
                    Hostname = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();
        }

        private string? ExtractDatasetKey(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            var host = uri.Host.ToLowerInvariant();
            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            // CBS detail: /detail/83648NED
            if (host.Contains("cbs.nl"))
            {
                var detailIndex = Array.FindIndex(segments,
                    s => s.Equals("detail", StringComparison.OrdinalIgnoreCase));

                if (detailIndex >= 0 && detailIndex + 1 < segments.Length)
                {
                    return $"CBS:{segments[detailIndex + 1]}";
                }

                // fragment parsing
                var fragment = uri.Fragment;

                if (!string.IsNullOrWhiteSpace(fragment))
                {
                    var datasetMatch = System.Text.RegularExpressions.Regex.Match(
                        fragment,
                        @"dataset\/([A-Za-z0-9_]+)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                        TimeSpan.FromMilliseconds(100));

                    if (datasetMatch.Success)
                    {
                        return $"CBS:{datasetMatch.Groups[1].Value}";
                    }

                    var tidMatch = System.Text.RegularExpressions.Regex.Match(
                        fragment,
                        @"tid=([A-Za-z0-9_]+)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                        TimeSpan.FromMilliseconds(100));

                    if (tidMatch.Success)
                    {
                        return $"CBS:{tidMatch.Groups[1].Value}";
                    }
                }

                return null;
            }

            // Statbank Denmark
            if (host.Contains("statbank.dk"))
            {
                if (segments.Length > 0)
                {
                    return $"StatbankDK:{segments[0].ToUpperInvariant()}";
                }

                return null;
            }

            // OECD
            if (host.Contains("oecd.org"))
            {
                var query = uri.Query;

                var match = System.Text.RegularExpressions.Regex.Match(
                    query,
                    @"df\[id\]=([^&]+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(100));

                if (match.Success)
                {
                    return $"OECD:{Uri.UnescapeDataString(match.Groups[1].Value)}";
                }

                return null;
            }

            return null;
        }

        private static bool MatchesLlmGroup(string provider, string? llmGroup)
        {
            if (string.IsNullOrWhiteSpace(llmGroup))
                return true;

            if (llmGroup.Equals("websearch disabled", StringComparison.OrdinalIgnoreCase))
            {
                return provider.StartsWith("gemini-2.5-flash-lite", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("gpt-4o-mini", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("grok-4.3", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("grok-4-1-fast-non-reasoning", StringComparison.OrdinalIgnoreCase);
            }

            if (llmGroup.Equals("websearch enabled", StringComparison.OrdinalIgnoreCase))
            {
                return provider.StartsWith("gemini-3.1-pro", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("gemini-2.5-pro", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("gpt-5.4", StringComparison.OrdinalIgnoreCase)
                    || provider.StartsWith("grok-4.20-reasoning", StringComparison.OrdinalIgnoreCase);
            }

            // fallback
            return provider.StartsWith(llmGroup, StringComparison.OrdinalIgnoreCase);
        }

        private Dictionary<string, MetricsOverTimeDto> GetWeeklyMetricsGrouped(List<FactCheckResult> results,string? nsi,string? llm,string? theme,Func<FactCheckResult, string> groupSelector)
        {
            var filtered = ApplyFilters(results, nsi, llm, theme);

            return filtered
                .GroupBy(groupSelector)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var weekly = g
                            .GroupBy(r =>
                            {
                                var date = r.ParsedModelResponse.ModelResponse.CreatedUtc.Date;
                                var diff = ((int)date.DayOfWeek + 6) % 7;

                                return date.AddDays(-diff);
                            })
                            .OrderBy(x => x.Key)
                            .ToList();

                        return new MetricsOverTimeDto
                        {
                            Accuracy = weekly.Select(w => new ChartPointDto
                            {
                                Label = w.Key.ToString("yyyy-MM-dd"),
                                Value = ComputeAccuracyMetric(w.ToList()).Score
                            }).ToList(),

                            Consistency = weekly.Select(w => new ChartPointDto
                            {
                                Label = w.Key.ToString("yyyy-MM-dd"),
                                Value = ComputeConsistencyMetric(w.ToList()).Score
                            }).ToList(),

                            Findability = weekly.Select(w => new ChartPointDto
                            {
                                Label = w.Key.ToString("yyyy-MM-dd"),
                                Value = ComputeFindabilityMetric(w.ToList()).Score
                            }).ToList()
                        };
                    });
        }

        public Dictionary<string, MetricsOverTimeDto> GetWeeklyMetricsPerNsi(List<FactCheckResult> results,string? nsi,string? llm,string? theme)
        {
            return GetWeeklyMetricsGrouped(
                results,
                nsi,
                llm,
                theme,
                r => r.ParsedModelResponse.ModelResponse.Prompt.Provider
            );
        }

        public Dictionary<string, MetricsOverTimeDto> GetWeeklyMetricsPerModel(List<FactCheckResult> results,string? nsi,string? llm,string? theme)
        {
            return GetWeeklyMetricsGrouped(
                results,
                nsi,
                llm,
                theme,
                r => r.ParsedModelResponse.ModelResponse.Provider
            );
        }

        public Dictionary<string, MetricsOverTimeDto> GetWeeklyMetricsPerTheme(List<FactCheckResult> results,string? nsi,string? llm,string? theme)
        {
            return GetWeeklyMetricsGrouped(
                results,
                nsi,
                llm,
                theme,
                r => r.ParsedModelResponse.ModelResponse.Prompt.Theme
            );
        }


        public class MetricResultDto
        {
            public double Score { get; set; }
            public string Tooltip { get; set; } = string.Empty;
        }
    }
}