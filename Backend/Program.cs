using AI_stats_measurement.Backend.Clients;
using AI_stats_measurement.Backend.Services;
using AI_stats_measurement.Clients;
using AI_stats_measurement.Data;
using AI_stats_measurement.Interface;
using AI_stats_measurement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AIMeasureDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));

//builder.Services.AddDbContext<AIMeasureDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<ILlmQuerier, ChatGPTQuerier>();
builder.Services.AddScoped<ILlmQuerier, ChatGPTWebSearchQuerier>();
builder.Services.AddScoped<ILlmQuerier, GeminiQuerier>();
builder.Services.AddScoped<ILlmQuerier, GeminiWebSearchQuerier>();
builder.Services.AddScoped<ILlmQuerier, GrokQuerier>();
builder.Services.AddScoped<ILlmQuerier, GrokWebSearchQuerier>();

builder.Services.AddScoped<LlmAggregator>();

builder.Services.AddScoped<FactChecker>(sp =>
    new FactChecker(0.05m, "CBS")
);

builder.Services.AddScoped<AnalyticsService>();

builder.Services.AddScoped<SourceNormalizer>();

builder.Services.AddScoped<EvaluationPipeline>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AIMeasureDbContext>();
    db.Database.Migrate();
}

//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");   

app.MapControllers();

app.Run();
