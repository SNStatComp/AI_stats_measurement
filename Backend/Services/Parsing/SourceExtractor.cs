using AI_stats_measurement.Backend.Enums;
using AI_stats_measurement.Backend.Helpers;
using AI_stats_measurement.Backend.Services.Classification;
using AI_stats_measurement.Backend.Models;
using System.Text.RegularExpressions;

namespace AI_stats_measurement.Backend.Services.Parsing
{
    public static class SourceExtractor
    {
        public static List<ExtractedSource> ExtractSources(
            string text,
            ParserLanguage language,
            Regex markdownLinkRegex,
            Regex urlRegex)
        {
            var sources = new List<ExtractedSource>();

            if (string.IsNullOrWhiteSpace(text))
                return sources;

            ExtractMarkdownLinks(text, sources, markdownLinkRegex);

            text = markdownLinkRegex.Replace(text, "");

            ExtractPlainUrls(text, sources, urlRegex);

            if (sources.Count == 0)
            {
                ExtractNamedSourcesFromLines(text, sources, GetSourceLabel(language));
            }

            return sources;
        }

        private static void ExtractMarkdownLinks(string text, List<ExtractedSource> sources, Regex markdownLinkRegex)
        {
            foreach (Match match in markdownLinkRegex.Matches(text))
            {
                var name = match.Groups[1].Value.Trim();
                var url = NormalizeUrl(match.Groups[2].Value);

                if (name.Contains("https", StringComparison.OrdinalIgnoreCase))
                    name = UrlHelper.GetSourceName(name);

                sources.Add(new ExtractedSource
                {
                    Name = Truncate(name, 512),
                    Url = Truncate(url, 2048),
                    Type = SourceClassifier.GetSourceType(url)
                });
            }
        }

        private static void ExtractPlainUrls(string text, List<ExtractedSource> sources, Regex urlRegex)
        {
            foreach (Match match in urlRegex.Matches(text))
            {
                var url = NormalizeUrl(match.Value);

                if (sources.Any(s => string.Equals(s.Url, url, StringComparison.OrdinalIgnoreCase)))
                    continue;

                sources.Add(new ExtractedSource
                {
                    Name = Truncate(UrlHelper.GetSourceName(url), 512),
                    Url = Truncate(url, 2048),
                    Type = SourceClassifier.GetSourceType(url)
                });
            }
        }

        private static void ExtractNamedSourcesFromLines(string text, List<ExtractedSource> sources, string sourceLabel)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim().Replace("**", "");
                var labelIndex = line.IndexOf(sourceLabel, StringComparison.OrdinalIgnoreCase);

                if (labelIndex < 0)
                    continue;

                var sourceText = line[(labelIndex + sourceLabel.Length)..].Trim();
                sourceText = sourceText.TrimEnd('.', ',', ';');

                var stop = sourceText.IndexOfAny(new[] { ',', '-', '–', '—', '.' });
                if (stop > -1)
                    sourceText = sourceText[..stop];

                if (string.IsNullOrWhiteSpace(sourceText))
                    continue;

                if (sources.Any(s => string.Equals(s.Name?.Trim(), sourceText, StringComparison.OrdinalIgnoreCase)))
                    continue;

                sources.Add(new ExtractedSource
                {
                    Name = Truncate(sourceText, 512),
                    Url = null,
                    Type = null
                });
            }
        }

        private static string NormalizeUrl(string url)
        {
            var normalized = url.Trim().TrimEnd('.', ',', ';');

            if (!normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                normalized = "https://" + normalized;

            return Truncate(normalized, 2048)!;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }

        private static string GetSourceLabel(ParserLanguage language)
            => language == ParserLanguage.Dutch ? "bron:" : "source:";
    }
}
