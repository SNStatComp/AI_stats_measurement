using AI_stats_measurement.Backend.Enums;
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

namespace AI_stats_measurement.Backend.Services.Parsing;

public static class ModelResponseParser
{
    private static readonly Regex MarkdownLinkRegex =
        new(@"\[([^\]]+)\]\(((https?:\/\/)?([a-z0-9\-]+\.)+[a-z]{2,}[^\s\)]*)\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex UrlRegex =
        new(@"(https?:\/\/)?([a-z0-9\-]+\.)+[a-z]{2,}(\/[^\s\)\]]*)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ParsedModelResponse ParseDutch(int responseId, string? rawText)
        => Parse(responseId, rawText, ParserLanguage.Dutch);

    public static ParsedModelResponse ParseEnglish(int responseId, string? rawText)
        => Parse(responseId, rawText, ParserLanguage.English);

    private static ParsedModelResponse Parse(int responseId, string? rawText, ParserLanguage language)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedModelResponse(responseId, 0, new List<ExtractedSource>());

        var text = rawText.Trim();

        var sources = SourceExtractor.ExtractSources(text, language, MarkdownLinkRegex, UrlRegex);
        var answer = NumberExtractor.ExtractNumber(text, language);

        return new ParsedModelResponse(responseId, answer ?? 0, sources);
    }
}
