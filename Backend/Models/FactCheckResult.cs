using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

public class FactCheckResult
{
    public int Id { get; init; }
    public int ParsedModelResponseId { get; private set; }
    public decimal SquareMeanRootError { get; private set; }
    public decimal RelativeError { get; private set; }
    public bool IsCorrect { get; private set; }
    public FactCheckResult(int parsedModelResponseId, decimal squareMeanRootError, decimal relativeError, bool isCorrect) { 
        ParsedModelResponseId = parsedModelResponseId;
        SquareMeanRootError = squareMeanRootError;
        RelativeError = relativeError;
        IsCorrect = isCorrect;
    }
}