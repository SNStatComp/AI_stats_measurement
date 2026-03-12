using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Models;
using System;
using System.Security.Policy;

public class Prompt
{
	public int Id { get; private set; }
    public string Instruction {  get; private set; } = null!;
    public string Theme { get; private set; } = null!;
	public DateTime Periode { get; private set; }
    public string Subject { get; private set; } = null!;
    public string Question { get; private set; } = null!;
	public decimal Answer { get; private set; }
	public string Source { get; private set; } = null!;
	public string AnswerLocation { get; private set; } = null!;
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;

    public List<PromptDimension> Dimensions { get; private set; } = new();
    public List<ModelResponse> ModelResponses { get; set; } = new();

    private Prompt() { } 

    public Prompt(string instruction, string theme, DateTime periode, string subject, string question, decimal answer, string source, string answerLocation)
	{
		Theme = theme;
		Periode = periode;
		Subject = subject;
        Instruction = instruction;
		Question = question;
		Answer = answer;
		Source = source;
		AnswerLocation = answerLocation;
    }

    public void AddDimension(string name, string value)
    {
        Dimensions.Add(new PromptDimension(Id, name, value));
    }
}
