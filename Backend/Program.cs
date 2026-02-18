using AI_stats_measurement.Clients;
using AI_stats_measurement.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<ILlmQuerier, ChatGPTQuerier>();
builder.Services.AddSingleton<ILlmQuerier, GeminiQuerier>();
builder.Services.AddSingleton<ILlmQuerier, GrokQuerier>();

var app = builder.Build();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
