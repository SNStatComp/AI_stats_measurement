using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Clients;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AIMeasureDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ILlmQuerier, ChatGPTQuerier>();
builder.Services.AddScoped<ILlmQuerier, GeminiQuerier>();
builder.Services.AddScoped<ILlmQuerier, GrokQuerier>();

builder.Services.AddScoped<LlmAggregator>();

builder.Services.AddScoped<FactChecker>(sp =>
    new FactChecker(0.05m)
);

builder.Services.AddScoped<EvaluationPipeline>();

var app = builder.Build();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
