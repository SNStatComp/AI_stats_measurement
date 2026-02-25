using AI_stats_measurement.Services;
using Xunit;

namespace AI_stats_measurement.Tests;

public class ModelResponseParserTests
{
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectly()
    {
        var text = "Eind 2020 waren er 827.000 huizen.";

        var (answer, source) = ModelResponseParser.Parse(text);

        Assert.Equal(827_000L, answer);
        Assert.Null(source);
    }

    [Fact]
    public void Parse_WithUrl_FillsSourceWithUrl()
    {
        var text = "Eind 2020 waren er 827.000 huizen. https://www.CBS.nl";

        var (answer, source) = ModelResponseParser.Parse(text);

        Assert.Equal(827_000L, answer);
        Assert.Equal("https://www.CBS.nl", source);
    }

    [Fact]
    public void Parse_WithSource_ExtractsSource()
    {
        var text = "Eind 2020 waren er 827.000 huizen. Bron: CBS StatLine";

        var (answer, source) = ModelResponseParser.Parse(text);

        Assert.Equal(827_000L, answer);
        Assert.Equal("CBS StatLine", source);
    }

    [Fact]
    public void Parse_MultipleNumbers_PicksFirstBest_NotYear()
    {
        var text = "Eind 2020 waren er in Nederland **794.000** lopende uitkeringen wegens arbeidsongeschiktheid (WAO, WIA, Wajong e.d.).\\n\\n**Bron:** CBS StatLine (https://opendata.cbs.nl/statline/#/CBS/nl/dataset/83648NED/table).\"";

        var (answer, source) = ModelResponseParser.Parse(text);

        Assert.Equal(794_000L, answer);
        Assert.Equal("https://opendata.cbs.nl/statline/#/CBS/nl/dataset/83648NED/table", source);
    }

    [Fact]
    public void Parse_Miljoen_IsScaledCorrectly()
    {
        var text = "De bevolking is 17,9 miljoen.";

        var (answer, source) = ModelResponseParser.Parse(text);

        Assert.Equal(17_900_000L, answer);
        Assert.Null(source);
    }
}
