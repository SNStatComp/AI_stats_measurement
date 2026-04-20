using AI_stats_measurement.Backend.Enums;

namespace AI_stats_measurement.Backend.Helpers
{
    public class SourceTypeMapper
    {     
        public static string ToValue(ParsedSourceType type) => type switch
        {
            ParsedSourceType.NsiDatabase => "NSI database",
            ParsedSourceType.NsiWebsite => "NSI website",
            ParsedSourceType.NsiNotSpecific => "NSI not specific",
            ParsedSourceType.ExternalPublication => "External publication",
            _ => "External publication"
        };
    }
}
