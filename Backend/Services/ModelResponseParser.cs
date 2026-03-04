using AI_stats_measurement.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AI_stats_measurement.Services;
public static class ModelResponseParser
{
    // Matches: 100 miljoen, 1,2 miljoen, 3.4 miljard, 12 duizend, 1.200.000, 1 200 000, 2020, 827000, 17,9
    private static readonly Regex NumberWithUnit =
        new Regex(
            @"(?ix)
            (?<num>
            \d+(?:[.,]\d+)?               # 2020, 827000, 17,9
            |
            \d{1,3}(?:[.\s]\d{3})+(?:[.,]\d+)?  # 827.000, 1 200 000, 1.234.567,89
            )
            \s*
            (?<unit>miljoen|miljard|duizend|mln|mjn)?
            ",
        RegexOptions.Compiled);

    // Matches: "bron: CBS Statline", "source: CBS Statline", **Bron:**
    private static readonly Regex SourceRegex =
        new Regex(
            @"(?im)^\s*\*{0,2}(bron|source)\*{0,2}\s*:\s*(?<src>.+)$" 
            , 
            RegexOptions.Compiled);

    // Matches: "https://cbs.nl"
    private static readonly Regex UrlRegex =
    new Regex(
        @"https?://[^\s\)\]]+",
        RegexOptions.Compiled
    );

    public static ParsedModelResponse Parse(int responseId,string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedModelResponse(0,"");

        var text = rawText.Trim();

        // Extract source
        string? source = null;
        var srcMatch = SourceRegex.Match(text);
        var urlMatch = UrlRegex.Match(text);
        if (urlMatch.Success)
        {
            source = urlMatch.Value;
        }
        else if (srcMatch.Success)
        {
            source = srcMatch.Groups["src"].Value.Trim();
        }

        // Find first numeric candidate
        long? best = null;

        foreach (Match m in NumberWithUnit.Matches(text))
        {
            if (!m.Success) continue;

            var numStr = m.Groups["num"].Value;
            var unitStr = m.Groups["unit"].Success
                ? m.Groups["unit"].Value.ToLowerInvariant()
                : null;

            if (!TryParseDutchNumber(numStr, out decimal value))
                continue;

            var multiplier = UnitMultiplier(unitStr);
            var scaled = value * multiplier;

            if (scaled < long.MinValue || scaled > long.MaxValue)
                continue;

            var candidate = (long)Math.Round(
                scaled, 0, MidpointRounding.AwayFromZero);

            // Skip likely years
            if (unitStr == null && IsLikelyYear(candidate))
                continue;

            // First meaningful number wins
            best = candidate;
            break;
        }

        return new ParsedModelResponse(Convert.ToDecimal(best), source);
    }

    private static decimal UnitMultiplier(string? unit)
    {
        return unit switch
        {
            "duizend" => 1_000m,
            "k" => 1_000m,
            "miljoen" => 1_000_000m,
            "mln" => 1_000_000m,
            "m" => 1_000_000m,      
            "miljard" => 1_000_000_000m,
            "bn" => 1_000_000_000m,
            _ => 1m
        };
    }

    private static bool TryParseDutchNumber(string input, out decimal value)
    {
        // Normalize thousand separators and decimal separators for NL formats:
        // "1.234,56" -> "1234.56"
        // "1 234,56" -> "1234.56"
        // "1,2" -> "1.2"
        var s = input.Trim();

        // remove spaces used as thousand separators
        s = s.Replace(" ", "");

        // If both '.' and ',' exist: assume '.' thousands and ',' decimals (NL style)
        if (s.Contains('.') && s.Contains(','))
        {
            s = s.Replace(".", "");
            s = s.Replace(",", ".");
        }
        else if (s.Contains(',') && !s.Contains('.'))
        {
            // only comma: treat as decimal separator
            s = s.Replace(",", ".");
        }
        else
        {
            // only dots or none: could be thousand separators (1.200.000) or decimals (3.14)
            // Heuristic: if dot is followed by exactly 3 digits repeatedly => thousands
            if (Regex.IsMatch(s, @"^\d{1,3}(\.\d{3})+$"))
                s = s.Replace(".", "");
        }

        return decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out value);
    }

    private static bool IsLikelyYear(long value)
    {
        return value >= 1900 && value <= 2050;
    }
}
