using AI_stats_measurement.Backend.Interface;
using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Data;
using AI_stats_measurement.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_stats_measurement.Tests
{
    public class EvaluationPipelineTests
    {
        [Fact]
        public async Task RunAsync_Creates_ModelResponse_ParsedResponse_FactCheck_And_ExportRow()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AIMeasureDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new AIMeasureDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var source = new Source
            {
                Name = "Test Source",
                Url = "https://example.com",
                Type = "NSI"
            };

            var prompt = new Prompt(
                "CBS",
                "test instruction",
                "test theme",
                DateTime.UtcNow,
                "test subject",
                "Hoeveel keer past 10 in 100?",
                10,
                source,
                ""
            );

            context.Prompts.Add(prompt);
            await context.SaveChangesAsync();

            var jobId = Guid.NewGuid();

            var response = new ModelResponse(
                prompt.Id,
                "gpt",
                "Het antwoord is 10.",
                null,
                null
            );

            var llmAggregatorMock = new Mock<ILlmAggregator>();

            llmAggregatorMock
                .Setup(x => x.AskByPromptIdsAsync(
                    It.IsAny<List<int>>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ModelResponse> { response });

            var pipeline = new EvaluationPipeline(
                llmAggregatorMock.Object,
                new FactChecker(0.05m, "CBS"),
                context,
                new SourceNormalizer(context)
            );

            var result = await pipeline.RunAsync(
                new List<int> { prompt.Id },
                new List<string> { "gpt" },
                jobId,
                CancellationToken.None
            );

            Assert.Single(result);

            Assert.Single(await context.ModelResponses.ToListAsync());
            Assert.Single(await context.ParsedModelResponses.ToListAsync());
            Assert.Single(await context.FactCheckResults.ToListAsync());
            Assert.Single(await context.ExportRows.ToListAsync());

            var row = result.First();

            Assert.Equal("test theme", row.Theme);
            Assert.Equal("Hoeveel keer past 10 in 100?", row.Question);
            Assert.Equal(10, row.ExpectedAnswer);
            Assert.Equal(10, row.ActualAnswer);
            Assert.Equal("gpt", row.Provider);
            Assert.True(row.AnswerIsCorrect);
        }

        [Fact]
        public async Task RecalculateAsync_Creates_New_ExportRows()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AIMeasureDbContext>()
                .UseSqlite(connection)
                .Options;

            await using var context = new AIMeasureDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var source = new Source
            {
                Name = "Test Source",
                Url = "https://example.com",
                Type = "NSI"
            };

            var prompt = new Prompt("CBS", "test", "test", DateTime.Now, "none", "Hoeveel keer past 10 in 100?", 10, source, "");
         
            context.Sources.Add(source);
            context.Prompts.Add(prompt);

            await context.SaveChangesAsync();

            var response = new ModelResponse(prompt.Id, "gpt", "Het antwoord is 10.", null, null);           

            context.ModelResponses.Add(response);
            await context.SaveChangesAsync();

            var pipeline = new EvaluationPipeline(
                llmAggregator: null!,
                checker: new FactChecker(0.05m, "CBS"),
                context: context,
                sourceNormalizer: new SourceNormalizer(context)
            );

            var result = await pipeline.RecalculateAsync(CancellationToken.None);

            Assert.Single(result);
            Assert.Single(context.ExportRows);
            Assert.Single(context.ParsedModelResponses);
            Assert.Single(context.FactCheckResults);

            var row = result.First();

            Assert.Equal("test", row.Theme);
            Assert.Equal("Hoeveel keer past 10 in 100?", row.Question);
            Assert.Equal(10, row.ExpectedAnswer);
            Assert.Equal(10, row.ActualAnswer);
            Assert.True(row.AnswerIsCorrect);
        }
    }
}
