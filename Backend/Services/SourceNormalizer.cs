using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Backend.Services
{
    public class SourceNormalizer
    {
        private readonly AIMeasureDbContext _context;

        public SourceNormalizer(AIMeasureDbContext context)
        {
            _context = context;
        }

        public async Task AttachNormalizedSourcesAsync(ParsedModelResponse parsed, CancellationToken ct)
        {
            var result = new List<ParsedModelResponseSource>();

            var seen = new HashSet<(string?, string?)>();

            foreach (var extracted in parsed.ExtractedSources)
            {
                var name = NormalizeSource(extracted.Name);
                var url = NormalizeUrl(extracted.Url);
                var type = extracted.Type;

                // skip duplicates early
                if (!seen.Add((name, url)))
                    continue;

                var source = await _context.Sources
                    .FirstOrDefaultAsync(s => s.Name == name && s.Url == url, ct);

                if (source == null)
                {
                    source = new Source
                    {
                        Name = name,
                        Type = type,
                        Url = url,                     
                    };

                    _context.Sources.Add(source);
                }

                result.Add(new ParsedModelResponseSource
                {
                    Source = source
                });
            }

            parsed.ParsedModelResponseSources = result;
        }

        private static string? NormalizeSource(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private static string? NormalizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            return url.Trim().TrimEnd('/');
        }
    }
}
