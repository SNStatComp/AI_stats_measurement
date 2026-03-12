using AI_stats_measurement.Models;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

public class FactCheckResult
{
    public int Id { get; init; }
    public int ParsedModelResponseId { get; private set; }
    public decimal SquareMeanRootError { get; private set; }
    public decimal RelativeError { get; private set; }
    public bool AnswerIsCorrect { get; private set; }
    public bool SourceIsCorrect { get; private set; }
    public decimal AverageRelativeError { get; private set; }
    public decimal AverageAnswer { get; private set; }
    public decimal AverageAnswerCorrectness { get; private set; }
    public decimal AverageSourceCorrectness { get; private set; }

    public ParsedModelResponse ParsedModelResponse { get; set; } = null!;

    private FactCheckResult() { }

    public FactCheckResult(
    int parsedModelResponseId,
    decimal squareMeanRootError,
    decimal relativeError,
    bool answerIsCorrect,
    bool sourceIsCorrect,
    decimal averageRelativeError,
    decimal averageAnswer,
    decimal averageAnswerCorrectness,
    decimal averageSourceCorrectness)
    {
        ParsedModelResponseId = parsedModelResponseId;
        SquareMeanRootError = squareMeanRootError;
        RelativeError = relativeError;
        AnswerIsCorrect = answerIsCorrect;
        SourceIsCorrect = sourceIsCorrect;
        AverageRelativeError = averageRelativeError;
        AverageAnswer = averageAnswer;
        AverageAnswerCorrectness = averageAnswerCorrectness;
        AverageSourceCorrectness = averageSourceCorrectness;
    }
}