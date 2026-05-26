using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services.Parsing;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Mono.TextTemplating;
using System.Diagnostics.Metrics;
using System.Runtime.ConstrainedExecution;
using Xunit;
using Xunit.Abstractions;
using static System.Net.WebRequestMethods;

namespace AI_stats_measurement.Tests;

public class ModelResponseParserTests
{
    [Fact]
    public void Parse_Extracts_DutchThousandNumber()
    {
        var text = "In 2020 was de gemiddelde verkoopprijs ongeveer € 348.000.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(348_000m, parsed.Answer);
    }

    [Fact]
    public void Parse_Extracts_MillionNumber()
    {
        var text = "In 2020 waren er ongeveer 1,1 miljoen uitkeringen.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(1_100_000m, parsed.Answer);
    }

    [Fact]
    public void Parse_Ignores_Year_As_Answer()
    {
        var text = "In 2020 waren er 68.500 studenten.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(68_500m, parsed.Answer);
    }

    [Fact]
    public void Parse_Extracts_Plain_Url_Source()
    {
        var text = "Meer informatie vind je hier: https://www.cbs.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Url == "https://www.cbs.nl");
    }

    [Fact]
    public void Parse_Extracts_Plain_Url_Name()
    {
        var text = "Meer informatie vind je hier: https://www.cbs.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "cbs");
    }

    [Fact]
    public void Parse_Extracts_Markdown_Link_Url()
    {
        var text = "Bron: [CBS](https://www.cbs.nl)";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Url == "https://www.cbs.nl");
    }

    [Fact]
    public void Parse_Extracts_Markdown_Link_Name()
    {
        var text = "Bron: [CBS](https://www.cbs.nl)";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "CBS");
    }

    [Fact]
    public void Parse_Extracts_Bron_Line_Text()
    {
        var text = "Bron: Kadaster.nl.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "kadaster");
    }

    [Fact]
    public void Parse_Extracts_Markdown_Over_Url()
    {
        var text = "[CBS](https://www.cbs.nl) Bron: CBS";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "CBS");
    }

    [Fact]
    public void Parse_Extracts_Markdown_Over_SourceLine()
    {
        var text = "[CBS](https://www.cbs.nl) https://www.cbs.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "CBS");
    }

    [Fact]
    public void Parse_Extracts_Markdown_AND_Url()
    {
        var text = "[CBS](https://www.cbs.nl) https://www.uwv.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "CBS");
        Assert.Contains(parsed.ExtractedSources, s => s.Name == "uwv");
    }

    [Fact]
    public void Parse_Extracts_Sourceline_Markdown_Name_Same_As_Url()
    {
        var text = "**Bron:** CBS, Tabel: Handel en diensten; omzet en productie. [https://www.cbs.nl](https://www.cbs.nl)";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "cbs");
    }


    [Fact]
    public void Parse_Extracts_CBS_Cijfers_SourceType_NSI_Website()
    {
        var text = "https://www.cbs.nl/nl-nl/cijfers/detail/83648NED";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI website");
    }

    [Fact]
    public void Parse_Extracts_SourceType_NSI_Not_Specific()
    {
        var text = "https://www.cbs.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI not specific");
    }

    [Fact]
    public void Parse_Extracts_CBS_Nieuws_SourceType_NSI_Website()
    {
        var text = "https://www.cbs.nl/nl-nl/nieuws/2026/09/eind-2025-1-1-procent-meer-mensen-met-bijstand";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI website");
    }

    [Fact]
    public void Parse_Extracts_SourceType_External_Publication()
    {
        var text = "https://www.uwv.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "External publication");
    }

    [Fact]
    public void Parse_Extracts_Miljoen_As_Full_Number()
    {
        var text = "De uitvoer bedroeg € 1.972 miljoen.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(1_972_000_000m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_When_No_Answer_Is_Found()
    {
        var text = "Ik heb geen specifieke data gevonden.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Source()
    {
        var text = "Bron: [CBS - Arbeidsongeschiktheidsuitkeringen] (https://www.cbs.nl/nl-nl/cijfers/86165NED/detail/arbeidsongeschiktheidsuitkeringen)";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Ignore_Year()
    {
        var text = "**De levensverwachting bij geboorte voor mannen in Nederland in 2022 was 80,1 jaar.**[[1]] (https://www.lifetable.de/File/GetDocument/data/NLD/NLD000020222022CU1.pdf)\r\n\r\nDit cijfer komt uit de officiële sterftetafels(levensverwachtingstafels) van het **Centraal Bureau voor de Statistiek(CBS)**. Ter vergelijking: in 2020 was het circa 79,7 jaar(daling door COVID-19), in 2024 circa 80,5 jaar.[[2]] (https://www.cbs.nl/?sc_itemid=40d28916-85d7-494e-84d6-9d97ca41e253&sc_lang=nl-%20nl)\r\n\r\n**Bron:** CBS, tabel 37360ned(Levensverwachting; geslacht, leeftijd).  \r\nDirecte link: [https://www.cbs.nl/nl-nl/cijfers/detail/37360ned](https://www.cbs.nl/nl-nl/cijfers/detail/37360ned) of de StatLine-tabel op opendata.cbs.nl.";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(80.1m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Recognize_Ton()
    {
        var text = "30.300 ton";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(30300000m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Dates()
    {
        var text = "1 januari 2021";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Age_Range_English()
    {
        var text = "aged 25 to 29 ";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Age_Range_Dutch()
    {
        var text = "18 tot 25 jaar";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Base_Year()
    {
        var text = "(base year 2025=100)";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Dates_English()
    {
        var text = "As of October 1, 2020";

        var parsed = ModelResponseParser.ParseEnglish(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Bold_Dates_English()
    {
        var text = "**January 1, 2020** (the first day of Q1 2020)";

        var parsed = ModelResponseParser.ParseEnglish(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Ignore_Short_Dates_English()
    {
        var text = "(table BOL103, data as of 1 Jan 2020) Denmark had 366,472 occupied";

        var parsed = ModelResponseParser.ParseEnglish(0, text);

        Assert.Equal(366472m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Approximately_English()
    {
        var text = "approximately 110,000–120,000";

        var parsed = ModelResponseParser.ParseEnglish(0, text);

        Assert.Equal(110000m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Age_Year_Range_English()
    {
        var text = "the median annual personal income for 25-29 year olds";

        var parsed = ModelResponseParser.ParseEnglish(0, text);

        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Ignore_Digit_Dutch()
    {
        var text = "(SBI-sector I, eerste digit 5)";

        var parsed = ModelResponseParser.ParseDutch(0, text);
        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Age_Range_Dutch()
    {
        var text = "**gemiddelde reisduur per persoon per dag voor 18- tot 25-jarigen in 2021: 75,12 minuten**";

        var parsed = ModelResponseParser.ParseDutch(0, text);
        Assert.Equal(75.12m, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Zero_Age_Range_English()
    {
        var text = "men and women aged 25-29";

        var parsed = ModelResponseParser.ParseEnglish(0, text);
        Assert.Equal(0, parsed.Answer);
    }

    [Fact]
    public void Parse_Returns_Answer_Age_Range_English()
    {
        var text = " the** median net wealth** for people aged **60–64** was** DKK 1,848,760**";

        var parsed = ModelResponseParser.ParseEnglish(0, text);
        Assert.Equal(1848760m, parsed.Answer);
    }
}
