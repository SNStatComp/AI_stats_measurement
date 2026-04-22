using Microsoft.Identity.Client;

namespace AI_stats_measurement.Backend.Models
{
    public class PromptDimension
    {
        public int Id { get; private set; }
        public int PromptId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Value { get; private set; } = null!;

        public Prompt Prompt { get; set; } = null!;

        private PromptDimension() { } 

        public PromptDimension(int promptId, string name, string value)
        {
            PromptId = promptId;
            Name = name;
            Value = value;
        }

        public static PromptDimension Import(int id, int promptId, string name, string value)
        {
            return new PromptDimension
            {
                Id = id,
                PromptId = promptId,
                Name = name,
                Value = value
            };
        }
    }
}
