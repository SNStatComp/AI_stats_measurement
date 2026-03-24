using AI_stats_measurement.Models;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

public class FactCheckResult
{
    public int Id { get; init; }
    public int ParsedModelResponseId { get; private set; }
    public decimal AbsoluteError { get; private set; }
    public decimal RelativeError { get; private set; }
    public bool AnswerIsCorrect { get; private set; }
    public bool SourceIsCorrect { get; private set; }
    public ParsedModelResponse ParsedModelResponse { get; set; } = null!;

    private FactCheckResult() { }

    public FactCheckResult(
    int parsedModelResponseId,
    decimal absoluteError,
    decimal relativeError,
    bool answerIsCorrect,
    bool sourceIsCorrect)
    {
        ParsedModelResponseId = parsedModelResponseId;
        AbsoluteError = absoluteError;
        RelativeError = relativeError;
        AnswerIsCorrect = answerIsCorrect;
        SourceIsCorrect = sourceIsCorrect;
    }
}