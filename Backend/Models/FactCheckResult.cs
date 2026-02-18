using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

public class FactCheckResult
{
    public int Id { get; init; }
    public int ParsedModelResponseId { get; private set; }
    public bool AnswerIsPresentInTable { get; set; }
    public double Score { get; private set; }
    public string? Reason { get; private set; }
    public int RunId { get; private set; }
    public DateTime DateTime { get; private set; }
    public FactCheckResult(int parsedModelResponseId, bool answerIsPresentInTable, double score, string reason, int runId) { 
        ParsedModelResponseId = parsedModelResponseId;
        AnswerIsPresentInTable = answerIsPresentInTable;
        Score = Score;
        Reason = reason;
        RunId = runId;
    }
}