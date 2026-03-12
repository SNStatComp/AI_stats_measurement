using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using System;
using System.Globalization;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace AI_stats_measurement.Services;

public class ModelResponseParser
{
    private static AIMeasureDbContext _context;

    ModelResponseParser(AIMeasureDbContext context)
    {
        _context = context;
    }

    // This regex matches URLs starting with http:// or https:// and continues until a whitespace, closing parenthesis, or closing square bracket is encountered.
    private static readonly Regex UrlRegex =
        new Regex(@"https?://[^\s\)\]]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Parses the raw text response from the model to extract a numeric answer and sources.
    public static ParsedModelResponse Parse(int responseId, string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedModelResponse(responseId,0 , new List<string>());

        var text = rawText.Trim();

        List<string> sources = ExtractSource(text);

        // Dutch culture
        var results = NumberRecognizer.RecognizeNumber(text, Culture.Dutch);

        decimal? best = null;

        foreach (var result in results)
        {
            // We are only interested in results that have a resolution with a "value" key that can be parsed as a decimal number.
            if (result.Resolution == null) continue;

            if (!result.Resolution.TryGetValue("value", out var valueObj)) continue;

            var valueText = valueObj?.ToString();
            if (string.IsNullOrWhiteSpace(valueText)) continue;

            // Try to parse the value as a decimal number using invariant culture to ensure consistent parsing regardless of locale.
            if (!decimal.TryParse(
                    valueText,
                    NumberStyles.Number | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                continue;
            }

            // skip likely years
            if (IsLikelyYear(value))
                continue;

            best = value;
            break;
        }

        var response = new ParsedModelResponse(responseId, best ?? 0, sources);  

        return response;     
    }

private static List<string> ExtractSource(string text)
    {
        var sources = new List<string>();

        foreach (Match match in UrlRegex.Matches(text))
        {
            var url = match.Value.Trim();

            // Do not add duplicate URLs
            if (!sources.Contains(url))
                sources.Add(url);
        }

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim().Replace("**", "");

            // Look for lines that contain "bron:" or "source:" (case-insensitive) and extract the text following the colon as a potential source.
            if (trimmed.Contains("bron:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("source:", StringComparison.OrdinalIgnoreCase))
            {
                var idx = trimmed.IndexOf(':');

                // If there is no colon or if the colon is at the end of the line, skip this line as it does not contain a valid source.
                if (idx < 0 || idx >= trimmed.Length - 1)
                    continue;

                var source = trimmed[(idx + 1)..].Trim();

                // If the source contains a comma or a dash, take only the part before the comma or dash as the source, as these characters often indicate additional information that is not part of the source name.
                var commaIndex = source.IndexOf(',');
                if (commaIndex >= 0) 
                    source = source[..commaIndex];

                var dashIndex = source.IndexOf('-');
                if (dashIndex >= 0)
                    source = source[..dashIndex];

                // remove trailing dots and trim whitespace
                sources.Add(source.Trim().TrimEnd('.'));
            }
        }

        return sources;
    }

    // This method checks if a given decimal value is likely to be a year, based on a reasonable range of years.
    private static bool IsLikelyYear(decimal value)
    {
        return value >= 1990 && value <= 2030;
    }
}
