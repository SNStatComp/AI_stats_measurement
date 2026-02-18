using System;
using System.Security.Policy;

public class Prompt
{
	public int Id { get; init; }
    public string Instruction {  get; private set; }
	public string Question { get; private set; }
	public string Answer { get; private set; }
	public string Source { get; private set; }
	public string AnswerLocation {get; private set;}
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;

    public Prompt(string instruction, string question, string answer, string source, string answerLocation)
	{
		Instruction = instruction;
		Question = question;
		Answer = answer;
		Source = source;
		AnswerLocation = answerLocation;
    }
}
