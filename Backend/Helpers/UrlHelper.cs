namespace AI_stats_measurement.Backend.Helpers
{
    public static class UrlHelper
    {
        public static string GetSourceName(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return url;

            var parts = uri.Host.Split('.');

            if (parts.Length == 0)
                return uri.Host;

            if (parts[0].Equals("www", StringComparison.OrdinalIgnoreCase) && parts.Length > 1)
                return parts[1];

            return parts[0];
        }
    }
}
