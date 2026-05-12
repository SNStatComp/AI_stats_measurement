using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_stats_measurement.Tests
{
    public class SourceNormalizerTests
    {
        [Fact]
        public async Task AttachNormalizedSourcesAsync_Trims_SourceName_And_Removes_TrailingSlash()
        {
            await using var context = CreateContext();
            var normalizer = new SourceNormalizer(context);

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 100,
                extractedSources: new List<ExtractedSource>
                {
                new ExtractedSource
                {
                    Name = " CBS ",
                    Url = "https://www.cbs.nl/",
                    Type = "NSI website"
                }
                }
            );

            await normalizer.AttachNormalizedSourcesAsync(parsed, CancellationToken.None);

            Assert.Single(parsed.ParsedModelResponseSources);
            Assert.Equal("CBS", parsed.ParsedModelResponseSources[0].Source.Name);
            Assert.Equal("https://www.cbs.nl", parsed.ParsedModelResponseSources[0].Source.Url);
        }

        [Fact]
        public async Task AttachNormalizedSourcesAsync_Skips_Duplicate_Sources()
        {
            await using var context = CreateContext();
            var normalizer = new SourceNormalizer(context);

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 100,
                extractedSources: new List<ExtractedSource>
                {
                new ExtractedSource
                {
                    Name = "CBS",
                    Url = "https://www.cbs.nl/",
                    Type = "NSI website"
                },
                new ExtractedSource
                {
                    Name = "CBS",
                    Url = "https://www.cbs.nl",
                    Type = "NSI website"
                }
                }
            );

            await normalizer.AttachNormalizedSourcesAsync(parsed, CancellationToken.None);

            Assert.Single(parsed.ParsedModelResponseSources);
        }

        [Fact]
        public async Task AttachNormalizedSourcesAsync_Reuses_Existing_Source()
        {
            await using var context = CreateContext();

            context.Sources.Add(new Source
            {
                Name = "CBS",
                Url = "https://www.cbs.nl",
                Type = "NSI website"
            });

            await context.SaveChangesAsync();

            var normalizer = new SourceNormalizer(context);

            var parsed = new ParsedModelResponse(
                modelResponseId: 1,
                answer: 100,
                extractedSources: new List<ExtractedSource>
                {
                new ExtractedSource
                {
                    Name = "CBS",
                    Url = "https://www.cbs.nl/",
                    Type = "NSI website"
                }
                }
            );

            await normalizer.AttachNormalizedSourcesAsync(parsed, CancellationToken.None);

            Assert.Single(parsed.ParsedModelResponseSources);
            Assert.Single(context.Sources);
        }

        private static AIMeasureDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AIMeasureDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AIMeasureDbContext(options);
        }
    }
}
