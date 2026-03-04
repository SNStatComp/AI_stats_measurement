namespace AI_stats_measurement.Backend.Models
{
    using System;

    namespace AI_stats_measurement.Backend.Models
    {
        public class ExportRow
        {
            public int Id { get; private set; }
            public string Theme { get; private set; } = null!;
            public string Question { get; private set; } = null!;
            public decimal ExpectedAnswer { get; private set; }
            public string ExpectedSource { get; private set; } = null!;
            public decimal ActualAnswer { get; private set; }
            public string ActualSource { get; private set; } = null!;
            public string Provider { get; private set; } = null!;
            public string? RawText { get; private set; }
            public string? Exception { get; private set; }
            public decimal SquareMeanRootError { get; private set; }
            public decimal RelativeError { get; private set; }
            public bool IsCorrect { get; private set; }
            public DateTime CreatedUtc { get; private set; }

            private ExportRow() { } 

            public ExportRow(
                string theme,
                string question,
                decimal expectedAnswer,
                string expectedSource,
                decimal actualAnswer,
                string actualSource,
                string provider,
                string? rawText,
                string? exception,
                decimal squareMeanRootError,
                decimal relativeError,
                bool isCorrect,
                DateTime createdUtc)
            {
                Theme = theme;
                Question = question;
                ExpectedAnswer = expectedAnswer;
                ExpectedSource = expectedSource;
                ActualAnswer = actualAnswer;
                ActualSource = actualSource;
                Provider = provider;
                RawText = rawText;
                Exception = exception;
                SquareMeanRootError = squareMeanRootError;
                RelativeError = relativeError;
                IsCorrect = isCorrect;
                CreatedUtc = createdUtc;
            }
        }
    }
}
