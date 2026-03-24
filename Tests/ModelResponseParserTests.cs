using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Services;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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

        Assert.Contains(parsed.ExtractedSources, s => s.Name == "Kadaster.nl");
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
    public void Parse_Extracts__SourceType_NSI_Database()
    {
        var text = "https://www.cbs.nl/nl-nl/cijfers/detail/83648NED";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI database");
    }

    [Fact]
    public void Parse_Extracts__SourceType_NSI_Not_Specific()
    {
        var text = "https://www.cbs.nl";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI not specific");
    }

    [Fact]
    public void Parse_Extracts__SourceType_NSI_Webarticle()
    {
        var text = "https://www.cbs.nl/nl-nl/nieuws/2026/09/eind-2025-1-1-procent-meer-mensen-met-bijstand";

        var parsed = ModelResponseParser.ParseDutch(0, text);

        Assert.Contains(parsed.ExtractedSources, s => s.Type == "NSI webarticle");
    }

    [Fact]
    public void Parse_Extracts__SourceType_External_Publication()
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
}
