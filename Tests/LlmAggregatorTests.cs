using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AI_stats_measurement.Tests;

public class LlmAggregatorTests
{
    private AIMeasureDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AIMeasureDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AIMeasureDbContext(options);
    }

    [Fact]
    public async Task AskByPromptIdsAsync_Calls_Only_Selected_Models_And_Prompts()
    {
        Source source = new Source { Id = 1, Name = "Test Source" };
        var prompt1 = new Prompt("CBS", "test", "test", DateTime.Now, "none", "question", 100, source, "");
        var prompt2 = new Prompt("CBS", "test", "test", DateTime.Now, "none", "question", 100, source, "");

        var context = CreateContext();
        context.Prompts.AddRange(prompt1, prompt2);
        await context.SaveChangesAsync();

        var mock1 = new Mock<ILlmQuerier>();
        mock1.SetupGet(q => q.Name).Returns("ChatGPT");
        mock1.Setup(q => q.AskAsync(It.IsAny<Prompt>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("antwoord");

        var mock2 = new Mock<ILlmQuerier>();
        mock2.SetupGet(q => q.Name).Returns("Gemini");
        mock2.Setup(q => q.AskAsync(It.IsAny<Prompt>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("antwoord");

        var aggregator = new LlmAggregator(
            new[] { mock1.Object, mock2.Object },
            context
        );

        var result = await aggregator.AskByPromptIdsAsync(
            new List<int> { 1, 2 },
            new List<string> { "ChatGPT" }, 
            CancellationToken.None
        );

        Assert.Equal(2, result.Count); 

        mock1.Verify(q => q.AskAsync(It.IsAny<Prompt>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        mock2.Verify(q => q.AskAsync(It.IsAny<Prompt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AskByPromptIdsAsync_Returns_Empty_When_No_Prompts_Found()
    {
        var context = CreateContext();

        var mock = new Mock<ILlmQuerier>();
        mock.SetupGet(q => q.Name).Returns("ChatGPT");

        var aggregator = new LlmAggregator(
            new[] { mock.Object },
            context
        );

        var result = await aggregator.AskByPromptIdsAsync(
            new List<int> { 999 }, 
            new List<string> { "ChatGPT" },
            CancellationToken.None
        );

        Assert.Empty(result);
    }

    [Fact]
    public async Task AskByPromptIdsAsync_Skips_Non_Selected_Models()
    {
        Source source = new Source { Id = 1, Name = "Test Source" };
        var prompt = new Prompt("CBS", "test", "test", DateTime.Now, "none", "question", 100, source, "");

        var context = CreateContext();
        context.Prompts.Add(prompt);
        await context.SaveChangesAsync();

        var mock = new Mock<ILlmQuerier>();
        mock.SetupGet(q => q.Name).Returns("Gemini");

        var aggregator = new LlmAggregator(
            new[] { mock.Object },
            context
        );

        var result = await aggregator.AskByPromptIdsAsync(
            new List<int> { 1 },
            new List<string> { "ChatGPT" },
            CancellationToken.None
        );

        Assert.Empty(result);
    }
}