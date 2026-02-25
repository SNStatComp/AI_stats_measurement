namespace AI_stats_measurement.Backend.Models
{
    public class PromptDimension
    {
        public int Id { get; private set; }
        public int PromptId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Value { get; private set; } = null!;

        private PromptDimension() { } 

        public PromptDimension(string name, string value, string? code = null)
        {
            Name = name;
            Value = value;
        }
    }
}
