using AI_stats_measurement.Backend.Services.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_stats_measurement.Tests
{
    public class AnalyticsServiceTest
    {
        [Fact]
        public void ComputeAccuracyScore_()
        {
            var text = "In 2020 was de gemiddelde verkoopprijs ongeveer € 348.000.";

            var parsed = ModelResponseParser.ParseDutch(0, text);

            Assert.Equal(348_000m, parsed.Answer);
        }
    }
}
