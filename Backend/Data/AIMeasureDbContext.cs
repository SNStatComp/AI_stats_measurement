using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Backend.Models.AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AI_stats_measurement.Data;

public class AIMeasureDbContext : DbContext
{
    public AIMeasureDbContext(DbContextOptions<AIMeasureDbContext> options)
        : base(options)
    {
    }

    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<PromptDimension> PromptDimensions => Set<PromptDimension>();
    public DbSet<ModelResponse> ModelResponses => Set<ModelResponse>();
    public DbSet<ParsedModelResponse> ParsedModelResponses => Set<ParsedModelResponse>();
    public DbSet<FactCheckResult> FactCheckResults => Set<FactCheckResult>();
    public DbSet<ExportRow> ExportRows => Set<ExportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PromptDimension>()
            .HasIndex(d => new { d.PromptId, d.Name })
            .IsUnique();

        modelBuilder.Entity<Prompt>()
            .HasMany(p => p.Dimensions)
            .WithOne(d => d.Prompt)
            .HasForeignKey(d => d.PromptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ModelResponse>()
            .HasOne(r => r.Prompt)
            .WithMany(p => p.ModelResponses)
            .HasForeignKey(r => r.PromptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParsedModelResponse>()
            .HasOne(p => p.ModelResponse)
            .WithOne(r => r.ParsedResponse)
            .HasForeignKey<ParsedModelResponse>(p => p.ModelResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FactCheckResult>()
            .HasOne(f => f.ParsedModelResponse)
            .WithOne(p => p.FactCheckResult)
            .HasForeignKey<FactCheckResult>(f => f.ParsedModelResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExportRow>();
    }
}

