using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace AI_stats_measurement.Services;

public class ModelResponseParser
{
    ModelResponseParser()
    {

    }

    private static readonly Regex MarkdownLinkRegex =
    new(@"\[([^\]]+)\]\((https?:\/\/[^\s\)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex UrlRegex =
        new(@"https?:\/\/[^\s\)\]]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);


    // Parses the raw text response from the model to extract a numeric answer and sources.
    public static ParsedModelResponse ParseDutch(int responseId, string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedModelResponse(responseId, 0, new List<ExtractedSource>());

        var text = rawText.Trim();

        var sources = ExtractDutchSources(text);

        var answer = ExtractDutchNumber(text);

        var response = new ParsedModelResponse(responseId, answer ?? 0, sources);  

        return response;     
    }

    // Parses the raw text response from the model to extract a numeric answer and sources.
    public static ParsedModelResponse ParseEnglish(int responseId, string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedModelResponse(responseId, 0, new List<ExtractedSource>());

        var text = rawText.Trim();

        var sources = ExtractEnglishSources(text);

        var answer = ExtractEnglishNumber(text);

        var response = new ParsedModelResponse(responseId, answer ?? 0, sources);
        return response;
    }

    private static List<ExtractedSource> ExtractDutchSources(string text)
    {
        var sources = new List<ExtractedSource>();

        if (string.IsNullOrWhiteSpace(text))
            return sources;

        // 1. Markdown: [naam](url)
        foreach (Match match in MarkdownLinkRegex.Matches(text))
        {
            var name = match.Groups[1].Value.Trim();
            var url = match.Groups[2].Value.Trim().TrimEnd('.', ',', ';');

            // If the name looks like a url, extract a cleaner name from the url
            if (name.Contains("https"))
                name = GetSourceName(name);

            sources.Add(new ExtractedSource
            {
                Name = name,
                Url = url,
                Type = SourceTypeHelper.GetSourceType(url)
            });
        }

        // Remove all markdown links from the text
        text = MarkdownLinkRegex.Replace(text, "");

        // 2. urls
        foreach (Match match in UrlRegex.Matches(text))
        {
            var url = match.Value.Trim().TrimEnd('.', ',', ';');

            // Check if this url was already added via markdown links
            if (sources.Any(s => string.Equals(s.Url, url, StringComparison.OrdinalIgnoreCase)))
                continue;

            var name = GetSourceName(url);

            sources.Add(new ExtractedSource
            {
                Name = name,
                Url = url,
                Type = SourceTypeHelper.GetSourceType(url)
            });
        }

        // 3. Sources mentioned in plain text, e.g. "Bron: CBS" or "Source: Rijksoverheid"
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim().Replace("**", "");

            var bronIndex = line.IndexOf("bron:", StringComparison.OrdinalIgnoreCase);
            if (bronIndex < 0)
                continue;

            var sourceText = line[(bronIndex + "bron:".Length)..].Trim();
            sourceText = sourceText.TrimEnd('.', ',', ';');

            if (string.IsNullOrWhiteSpace(sourceText))
                continue;

            // skip if already found from markdown/url
            if (sources.Any(s => string.Equals(s.Name?.Trim(), sourceText, StringComparison.OrdinalIgnoreCase)))
                continue;

            sources.Add(new ExtractedSource
            {
                Name = sourceText,
                Url = null,
                Type = null
            });
        }

        return sources;
    }

    private static List<ExtractedSource> ExtractEnglishSources(string text)
    {
        var sources = new List<ExtractedSource>();

        if (string.IsNullOrWhiteSpace(text))
            return sources;

        // 1. Markdown: [naam](url)
        foreach (Match match in MarkdownLinkRegex.Matches(text))
        {
            var name = match.Groups[1].Value.Trim();
            var url = match.Groups[2].Value.Trim().TrimEnd('.', ',', ';');

            // If the name looks like a url, extract a cleaner name from the url
            if (name.Contains("https"))
                name = GetSourceName(name);

            sources.Add(new ExtractedSource
            {
                Name = name,
                Url = url,
                Type = SourceTypeHelper.GetSourceType(url)
            });
        }

        // Remove all markdown links from the text
        text = MarkdownLinkRegex.Replace(text, "");

        // 2. urls
        foreach (Match match in UrlRegex.Matches(text))
        {
            var url = match.Value.Trim().TrimEnd('.', ',', ';');

            // Check if this url was already added via markdown links
            if (sources.Any(s => string.Equals(s.Url, url, StringComparison.OrdinalIgnoreCase)))
                continue;

            var name = GetSourceName(url);

            sources.Add(new ExtractedSource
            {
                Name = name,
                Url = url,
                Type = SourceTypeHelper.GetSourceType(url)
            });
        }

        // 3. Sources mentioned in plain text, e.g. "Bron: CBS" or "Source: Rijksoverheid"
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim().Replace("**", "");

            var bronIndex = line.IndexOf("source:", StringComparison.OrdinalIgnoreCase);
            if (bronIndex < 0)
                continue;

            var sourceText = line[(bronIndex + "source:".Length)..].Trim();
            sourceText = sourceText.TrimEnd('.', ',', ';');

            if (string.IsNullOrWhiteSpace(sourceText))
                continue;

            // skip if already found from markdown/url
            if (sources.Any(s => string.Equals(s.Name?.Trim(), sourceText, StringComparison.OrdinalIgnoreCase)))
                continue;

            sources.Add(new ExtractedSource
            {
                Name = sourceText,
                Url = null,
                Type = null
            });
        }

        return sources;
    }

    // Extract numbers in Dutch format.
    private static decimal? ExtractDutchNumber(string text)
    {
        text = CleanText(text);

        var matches = Regex.Matches(
            text,
            @"(\d{1,3}(?:[.\s]\d{3})*(?:,\d+)?)\s*(miljoen|duizend|miljard)?",
            RegexOptions.IgnoreCase);

        decimal? best = null;

        foreach (Match match in matches)
        {
            var numberText = match.Groups[1].Value;

            // normalize Dutch format
            numberText = numberText.Replace(".", "").Replace(",", ".");

            if (!decimal.TryParse(numberText, NumberStyles.Number,
                CultureInfo.InvariantCulture, out var value))
                continue;

            var magnitude = match.Groups[2].Value.ToLowerInvariant();

            switch (magnitude)
            {
                case "duizend":
                    value *= 1_000m;
                    break;
                case "ton":
                    value *= 1_000m;
                    break;
                case "miljoen":
                    value *= 1_000_000m;
                    break;
                case "miljard":
                    value *= 1_000_000_000m;
                    break;
            }

            if (best == null || value > best)
                best = value;
        }

        return best;
    }

    // Extract numbers in English format using Microsoft Recognizers Text library.
    private static decimal? ExtractEnglishNumber(string text)
    {
        text = CleanText(text);

        var results = NumberRecognizer.RecognizeNumber(text, Culture.English);

        decimal? best = null;

        foreach (var result in results)
        {
            if (result.Resolution == null)
                continue;

            if (!result.Resolution.TryGetValue("value", out var valueObj))
                continue;

            var valueText = valueObj?.ToString();
            if (string.IsNullOrWhiteSpace(valueText))
                continue;

            if (!decimal.TryParse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                continue;

            if (best == null || value > best)
                best = value;
        }

        return best;
    }


    private static string CleanText(string text)
    {
        text = Regex.Replace(text, @"https?:\/\/\S+", ""); // remove urls
        text = Regex.Replace(text, @"\*\*", "");           // markdown bold
        text = Regex.Replace(text, @"\bin\s+(19|20)\d{2}\b", "", RegexOptions.IgnoreCase); // remove years "in 2021"
        text = Regex.Replace(text, @"\b(19|20)\d{2}-\w+", "", RegexOptions.IgnoreCase); // remove patterns like "2020/2021"
        text = Regex.Replace(text, @"\b(19|20)\d{2}\b", ""); // remove "2021-cijfers"
        text = Regex.Replace(text, @"^\d{4}\s*/\s*\d{4}$", ""); // remove standalone year ranges like "1990/2000"

        return text;
    }

    private static string GetSourceName(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        var parts = uri.Host.Split('.');

        if (parts.Length == 0)
            return uri.Host;

        if (parts[0] == "www" && parts.Length > 1)
            return parts[1];

        return parts[0];
    }

    public static class SourceTypeHelper
    {
        public static string GetSourceType(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            url = url.Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            var host = uri.Host.ToLowerInvariant();
            var path = uri.AbsolutePath.ToLowerInvariant();
            var fullUrl = url.ToLowerInvariant();

            // CBS / NSI database
            if (host.Contains("opendata.cbs.nl") ||
                host.Contains("ec.europa.eu") ||
                host.Contains("stats.oecd.org") ||
                host.Contains("data-explorer.oecd.org")
                )
                return "NSI database";

            // CBS / NSI webartikel
            if (host.EndsWith("cbs.nl") || host.EndsWith("longreads.cbs.nl"))
            {
                // check for typical news/article paths
                if (path.Contains("/nieuws/") ||
                    path.Contains("/news/") ||
                    path.Contains("/cijfers/detail/") ||
                    path.Contains("/visualisaties/") ||
                    path.Contains("/figures/") 
                    )
                {
                    return "NSI website";
                }

                return "NSI not specific";
            }

            // OECD
            if (host.EndsWith("oecd.org"))
            {
                // check for typical news/article paths
                if (path.Contains("/publications/") ||
                    path.Contains("/topics/")
                    )
                {
                    return "NSI website";
                }

                return "NSI not specific";
            }

            // StatBank Denmark
            if (host.EndsWith("oecd.org"))
                {
                    // check for typical news/article paths
                    if (path.Contains("/nieuws/") ||
                        path.Contains("/news/") ||
                        path.Contains("/cijfers/detail/") ||
                        path.Contains("/visualisaties/")
                        )
                    {
                        return "NSI website";
                    }

                    return "NSI not specific";
                }

            // INsee / NSI webartikel
            if (host.EndsWith("insee.fr"))
            {
                // check for typical news/article paths
                if (path.Contains("/statistiques/") 
                    )
                {
                    return "NSI website";
                }

                return "NSI not specific";
            }

            // Rijksoverheid / Eurostat / Worldbank / etc. webartikel
            return "External publication";
        }
    }
}
