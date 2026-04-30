namespace AI_stats_measurement.Backend.Dto
{
    public class PromptTransferDto : PromptDto
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class SourceTransferDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }
    }

    public class PromptDimensionTransferDto
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public string Name { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class ModelResponseTransferDto
    {
        public int Id { get; set; }
        public int PromptId { get; set; }
        public string Provider { get; set; } = null!;
        public string? RawText { get; set; }
        public string? Exception { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class DataExportBundleDto
    {
        public List<SourceTransferDto> Sources { get; set; } = new();
        public List<PromptTransferDto> Prompts { get; set; } = new();
        public List<PromptDimensionTransferDto> PromptDimensions { get; set; } = new();
        public List<ModelResponseTransferDto> ModelResponses { get; set; } = new();
    }

    public class DataExportAllBundleDto
    {
        public List<SourceTransferDto> Sources { get; set; } = new();
        public List<PromptTransferDto> Prompts { get; set; } = new();
        public List<PromptDimensionTransferDto> PromptDimensions { get; set; } = new();
        public List<ModelResponseTransferDto> ModelResponses { get; set; } = new();

        public List<ParsedModelResponseTransferDto> ParsedModelResponses { get; set; } = new();
        public List<FactCheckResultTransferDto> FactCheckResults { get; set; } = new();
        public List<ParsedModelResponseSourceTransferDto> ParsedModelResponseSources { get; set; } = new();
    }

    public class ParsedModelResponseTransferDto
    {
        public int Id { get; set; }
        public int ModelResponseId { get; set; }
        public decimal Answer { get; set; }
    }

    public class FactCheckResultTransferDto
    {
        public int Id { get; set; }
        public int ParsedModelResponseId { get; set; }
        public decimal AbsoluteError { get; set; }
        public decimal RelativeError { get; set; }
        public bool AnswerIsCorrect { get; set; }
        public bool SourceIsCorrect { get; set; }
        public bool Abstained { get; set; }
    }

    public class ParsedModelResponseSourceTransferDto
    {
        public int ParsedModelResponseId { get; set; }
        public int SourceId { get; set; }
    }
}
