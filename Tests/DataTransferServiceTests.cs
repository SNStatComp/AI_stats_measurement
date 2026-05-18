using AI_stats_measurement.Backend.Dto;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace AI_stats_measurement.Tests.Services;

public class DataTransferServiceTests
{
    private static AIMeasureDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AIMeasureDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x =>
                x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AIMeasureDbContext(options);
    }

    private static DataTransferService CreateService(AIMeasureDbContext context)
    {
        var environmentMock = new Mock<IWebHostEnvironment>();

        return new DataTransferService(
            context,
            environmentMock.Object
        );
    }

    [Fact]
    public async Task ExportAsync_ReturnsSourcesPromptsDimensionsAndModelResponses()
    {
        await using var context = CreateContext();

        var source = new Source
        {
            Id = 1,
            Name = "CBS",
            Url = "https://www.cbs.nl",
            Type = "Website"
        };

        context.Sources.Add(source);

        var prompt = Prompt.Import(
            1,
            "OpenAI",
            "Instruction",
            "Theme",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc),
            "Subject",
            "Question",
            100,
            1,
            "AnswerLocation",
            DateTime.UtcNow
        );

        context.Prompts.Add(prompt);

        context.Set<PromptDimension>().Add(
            PromptDimension.Import(1, 1, "Regio", "Nederland")
        );

        context.Set<ModelResponse>().Add(
            ModelResponse.Import(
                1,
                1,
                "OpenAI",
                "Raw response",
                null,
                DateTime.UtcNow
            )
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExportAsync();

        Assert.Single(result.Sources);
        Assert.Single(result.Prompts);
        Assert.Single(result.PromptDimensions);
        Assert.Single(result.ModelResponses);

        Assert.Equal("CBS", result.Sources[0].Name);
        Assert.Equal("Question", result.Prompts[0].Question);
        Assert.Equal("Regio", result.PromptDimensions[0].Name);
        Assert.Equal("Raw response", result.ModelResponses[0].RawText);
    }

    [Fact]
    public async Task ImportAsync_AddsDataToDatabase()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var bundle = new DataExportBundleDto
        {
            Sources =
            [
                new SourceTransferDto
                {
                    Id = 1,
                    Name = "CBS",
                    Url = "https://www.cbs.nl",
                    Type = "Website"
                }
            ],
            Prompts =
            [
                new PromptTransferDto
                {
                    Id = 1,
                    Provider = "OpenAI",
                    Instruction = "Instruction",
                    Theme = "Theme",
                    Periode = new DateTime(2026, 1, 1),
                    Subject = "Subject",
                    Question = "Question",
                    Answer = 100,
                    SourceId = 1,
                    AnswerLocation = "AnswerLocation",
                    CreatedUtc = DateTime.UtcNow
                }
            ],
            PromptDimensions =
            [
                new PromptDimensionTransferDto
                {
                    Id = 1,
                    PromptId = 1,
                    Name = "Regio",
                    Value = "Nederland"
                }
            ],
            ModelResponses =
            [
                new ModelResponseTransferDto
                {
                    Id = 1,
                    PromptId = 1,
                    Provider = "OpenAI",
                    RawText = "Raw response",
                    Exception = null,
                    CreatedUtc = DateTime.UtcNow
                }
            ]
        };

        await service.ImportAsync(bundle);

        Assert.Equal(1, await context.Sources.CountAsync());
        Assert.Equal(1, await context.Prompts.CountAsync());
        Assert.Equal(1, await context.Set<PromptDimension>().CountAsync());
        Assert.Equal(1, await context.Set<ModelResponse>().CountAsync());
    }

    [Fact]
    public async Task ImportAsync_DoesNotImportDuplicates()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var bundle = new DataExportBundleDto
        {
            Sources =
            [
                new SourceTransferDto
                {
                    Id = 1,
                    Name = "CBS",
                    Url = "https://www.cbs.nl",
                    Type = "Website"
                }
            ],
            Prompts = [],
            PromptDimensions = [],
            ModelResponses = []
        };

        await service.ImportAsync(bundle);
        await service.ImportAsync(bundle);

        Assert.Equal(1, await context.Sources.CountAsync());
    }

    [Fact]
    public async Task ExportAllAsync_ReturnsAllExportData()
    {
        await using var context = CreateContext();

        context.Sources.Add(new Source
        {
            Id = 1,
            Name = "CBS",
            Url = "https://www.cbs.nl",
            Type = "Website"
        });

        var prompt = Prompt.Import(
            1,
            "OpenAI",
            "Instruction",
            "Theme",
            DateTime.UtcNow,
            "Subject",
            "Question",
            100,
            1,
            "AnswerLocation",
            DateTime.UtcNow
        );

        context.Prompts.Add(prompt);

        var modelResponse = ModelResponse.Import(
            1,
            1,
            "OpenAI",
            "Raw response",
            null,
            DateTime.UtcNow
        );

        context.Set<ModelResponse>().Add(modelResponse);

        context.Set<ParsedModelResponse>().Add(new ParsedModelResponse(
            modelResponseId: 1,
            answer: 123,
            extractedSources: new List<ExtractedSource>()
            )
        );

        context.Set<FactCheckResult>().Add(
        new FactCheckResult(
            parsedModelResponseId: 1,
            absoluteError: 0,
            relativeError: 0,
            answerIsCorrect: true,
            sourceIsCorrect: true,
            abstained: false
        )
    );

        context.Set<ParsedModelResponseSource>().Add(new ParsedModelResponseSource
        {
            ParsedModelResponseId = 1,
            SourceId = 1
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.ExportAllAsync();

        Assert.Single(result.Sources);
        Assert.Single(result.Prompts);
        Assert.Single(result.ModelResponses);
        Assert.Single(result.ParsedModelResponses);
        Assert.Single(result.FactCheckResults);
        Assert.Single(result.ParsedModelResponseSources);
    }
}
