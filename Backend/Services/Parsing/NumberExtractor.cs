using AI_stats_measurement.Backend.Enums;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AI_stats_measurement.Backend.Services.Parsing
{
    public static class NumberExtractor
    {
        public static decimal? ExtractNumber(string text, ParserLanguage language)
        {
            var cleaned = TextCleaner.Clean(text);

            return language switch
            {
                ParserLanguage.Dutch => ExtractDutchNumber(cleaned),
                ParserLanguage.English => ExtractEnglishNumber(cleaned),
                _ => null
            };
        }

        private static decimal? ExtractDutchNumber(string text)
        {
            var matches = Regex.Matches(
                text,
                @"(\d{1,3}(?:[.\s]\d{3})*(?:,\d+)?)\s*(miljoen|duizend|miljard|ton)?",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(100));

            foreach (Match match in matches)
            {
                var numberText = match.Groups[1].Value.Replace(".", "").Replace(",", ".");

                if (!decimal.TryParse(numberText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                    continue;

                var magnitude = match.Groups[2].Value.ToLowerInvariant();

                value = magnitude switch
                {
                    "duizend" => value * 1_000m,
                    "ton" => value * 1_000m,
                    "miljoen" => value * 1_000_000m,
                    "miljard" => value * 1_000_000_000m,
                    _ => value
                };

                return value;
            }

            return null;
        }

        private static decimal? ExtractEnglishNumber(string text)
        {
            var results = NumberRecognizer.RecognizeNumber(text, Culture.English);

            foreach (var result in results)
            {
                if (result.Resolution == null)
                    continue;

                if (!result.Resolution.TryGetValue("value", out var valueObj))
                    continue;

                var valueText = valueObj?.ToString();
                if (string.IsNullOrWhiteSpace(valueText))
                    continue;

                if (decimal.TryParse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                    return value;
            }

            return null;
        }
    }
}
