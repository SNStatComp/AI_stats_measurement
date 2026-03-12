using AI_stats_measurement.Services;
using Xunit;
using static System.Net.WebRequestMethods;

namespace AI_stats_measurement.Tests;

public class ModelResponseParserTests
{
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectlyOpenAI()
    {
        var text = "In 2020 was de gemiddelde verkoopprijs van bestaande koopwoningen in Nederland ongeveer € 348.000. Deze prijs is een indicatie en kan variëren afhankelijk van de regio en het type woning. Voor de meest actuele informatie en gedetailleerde cijfers kun je de website van het Kadaster raadplegen. \n\nBron: Kadaster.nl.";
        List<string> predictedSources = ["Kadaster.nl"]; 

        var parsed = ModelResponseParser.Parse(0,text);

        Assert.Equal(348_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectly1Gemini()
    {
        var text = "Het gemiddelde verkoopprijs van bestaande koopwoningen in Nederland was in 2020 **€ 321.000**.\\n\\nBron: [CBS: Huizenprijzen 2020](https://www.cbs.nl/nl-nl/visualisaties/dashboard-huizenprijzen)";
        List<string> predictedSources = ["https://www.cbs.nl/nl-nl/visualisaties/dashboard-huizenprijzen", "[CBS: Huizenprijzen 2020](https://www.cbs.nl/nl"];

        var parsed = ModelResponseParser.Parse(0, text);

        Assert.Equal(321_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectlyGrok()
    {
        var text = "De gemiddelde verkoopprijs van bestaande koopwoningen in Nederland in 2020 was **€ 299.000**.\n\n**Bron:** Kadaster (Nederlands Kadaster), jaarrapportage woningmarkt 2020. Zie [kadaster.nl](https://www.kadaster.nl/documenten/rapportages/woningmarktcijfers/2020).";
        List<string> predictedSources = ["https://www.kadaster.nl/documenten/rapportages/woningmarktcijfers/2020", "Kadaster (Nederlands Kadaster)"];

        var parsed = ModelResponseParser.Parse(0, text);

        Assert.Equal(299_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectlyOPenAi1()
    {
        var text = "In 2020 waren er in Nederland ongeveer 1,1 miljoen lopende arbeidsongeschiktheidsuitkeringen op grond van de Wet langdurige zorg (Wlz) en andere arbeidsongeschiktheidswetten. Dit cijfer kan variëren afhankelijk van de bron en de gebruikte definities.\n\nVoor de meest actuele en gedetailleerde informatie raad ik aan om de website van het Centraal Bureau voor de Statistiek (CBS) te raadplegen. Daar vind je uitgebreide statistieken over arbeidsongeschiktheid in Nederland. \n\nMeer informatie vind je hier: [CBS](https://www.cbs.nl)";
        List<string> predictedSources = ["https://www.cbs.nl"];

        var parsed = ModelResponseParser.Parse(0, text);

        Assert.Equal(1_100_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
    [Fact]
    public void Parse_ParsesDutchThousandsCorrectlyGemini1()
    {
        var text = "In 2020 waren er in Nederland **732.000** lopende uitkeringen op grond van de Arbeidsongeschiktheidswetten (WAO/WIA).\n\nBron: CBS (Centraal Bureau voor de Statistiek) - [Arbeidsongeschiktheid; uitkeringen en arbeidsongeschikten](https://www.cbs.nl/nl-nl/cijfers/detail/arbeidsongeschiktheid-uitkeringen-en-arbeidsongeschikten) (Onder de tabel 'Uitkeringen op grond van de Arbeidsongeschiktheidswetten')";
        List<string> predictedSources = ["https://www.cbs.nl/nl-nl/cijfers/detail/arbeidsongeschiktheid-uitkeringen-en-arbeidsongeschikten", "CBS (Centraal Bureau voor de Statistiek)",];

        var parsed = ModelResponseParser.Parse(0, text);

        Assert.Equal(732_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
    [Fact]
    public void Parse_ParsesDutchThousandsGrok1()
    {
        var text = "In 2020 waren er **ongeveer 400.000 lopende Arbeidsongeschiktheidsuitkeringen (AOW-uitkeringen)** in Nederland op grond van de Wet arbeidsongeschiktheidsverzekering zelfstandigen (Waarz) en de Wet op de arbeidsongeschiktheidsverzekering (WAO), plus circa **800.000 uitkeringen onder de Wet werk en inkomen naar arbeidsvermogen (WIA)**. Dit zijn de hoofdwetgevingen voor arbeidsongeschiktheid.\n\n**Bron:** CBS StatLine (Centraal Bureau voor de Statistiek), dataset \"Uitkeringsontvangers per uitkeringssoort\" (gepubliceerd 2021), raadpleegbaar via [https://www.cbs.nl/nl-nl/cijfers/detail/83648NED](https://www.cbs.nl/nl-nl/cijfers/detail/83648NED). Exacte aantallen kunnen licht variëren per peildatum (bijv. eind 2020: WIA ~805.000, WAO/Waarz ~398.000).";
        List<string> predictedSources = ["https://www.cbs.nl/nl-nl/cijfers/detail/83648NED", "CBS StatLine (Centraal Bureau voor de Statistiek)"];

        var parsed = ModelResponseParser.Parse(0, text);

        Assert.Equal(400_000L, parsed.Answer);
        Assert.Equal(predictedSources, parsed.Sources);
    }
}
