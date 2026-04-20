using AI_stats_measurement.Backend.Enums;
using AI_stats_measurement.Backend.Helpers;

namespace AI_stats_measurement.Backend.Services.Classification
{
    public static class SourceClassifier
    {
        public static string? GetSourceType(string? url)
        {
            var sourceType = Classify(url);
            return sourceType is null ? null : SourceTypeMapper.ToValue(sourceType.Value);
        }

        public static ParsedSourceType? Classify(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            url = url.Trim();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;

            var host = uri.Host.ToLowerInvariant();
            var path = uri.AbsolutePath.ToLowerInvariant();

            if (host.Contains("opendata.cbs.nl"))
                return ParsedSourceType.NsiDatabase;

            if (host.Contains("ec.europa.eu"))
                return ParsedSourceType.NsiDatabase;

            if (host.Contains("stats.oecd.org") || host.Contains("data-explorer.oecd.org"))
                return ParsedSourceType.NsiDatabase;

            if (host.Contains("statistikbanken.dk") || host.Contains("statbank.dk"))
                return ParsedSourceType.NsiDatabase;

            if (host.EndsWith("cbs.nl"))
            {
                if (path.Contains("/nieuws/") ||
                    path.Contains("/news/") ||
                    path.Contains("/cijfers/detail/") ||
                    path.Contains("/visualisaties/") ||
                    path.Contains("/figures/") ||
                    path.Length > 1)
                {
                    return ParsedSourceType.NsiWebsite;
                }

                return ParsedSourceType.NsiNotSpecific;
            }

            if (host.EndsWith("oecd.org"))
            {
                if (path.Contains("/publications/") ||
                    path.Contains("/topics/") ||
                    path.Length > 1)
                {
                    return ParsedSourceType.NsiWebsite;
                }

                return ParsedSourceType.NsiNotSpecific;
            }

            if (host.EndsWith("dst.dk"))
            {
                if (path.Contains("/statistik/") ||
                    path.Contains("/statistics/") ||
                    path.Length > 1)
                {
                    return ParsedSourceType.NsiWebsite;
                }

                return ParsedSourceType.NsiNotSpecific;
            }

            if (host.EndsWith("insee.fr"))
            {
                if (path.Contains("/statistiques/"))
                    return ParsedSourceType.NsiWebsite;

                return ParsedSourceType.NsiNotSpecific;
            }

            return ParsedSourceType.ExternalPublication;
        }
    }
}
