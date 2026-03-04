using AI_stats_measurement.Services;
using Xunit;

namespace AI_stats_measurement.Tests;

public class ModelResponseParserTests
{
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectly()
    {
        var text = "Eind 2020 waren er 827.000 huizen.";

        var parsed = ModelResponseParser.Parse(0,text);

        Assert.Equal(827_000L, parsed.Answer);
        Assert.Null(parsed.Source);
    }   
}
