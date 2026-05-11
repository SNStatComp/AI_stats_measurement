using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Data;

public class AIMeasureDbContext : IdentityDbContext<ApplicationUser>
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
    public DbSet<Source> Sources => Set<Source>();  
    public DbSet<ParsedModelResponseSource> ParsedModelResponseSources => Set<ParsedModelResponseSource>();
    public DbSet<ExportRow> ExportRows => Set<ExportRow>();
    public DbSet<LlmJob> LlmJobs => Set<LlmJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PromptDimension>()
            .HasIndex(d => new { d.PromptId, d.Name })
            .IsUnique();

        modelBuilder.Entity<Prompt>()
            .HasMany(p => p.Dimensions)
            .WithOne(d => d.Prompt)
            .HasForeignKey(d => d.PromptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Prompt>()
            .HasOne(p => p.Source)
            .WithMany(s => s.Prompts)
            .HasForeignKey(p => p.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<Source>()
            .HasIndex(s => new { s.Name, s.Url })
            .IsUnique();

        modelBuilder.Entity<Source>()
            .Property(x => x.Url)
            .HasMaxLength(2048);

        modelBuilder.Entity<Source>()
            .Property(x => x.Name)
            .HasMaxLength(512);

        modelBuilder.Entity<ParsedModelResponseSource>()
            .HasKey(ps => new { ps.ParsedModelResponseId, ps.SourceId });

        modelBuilder.Entity<ParsedModelResponseSource>()
            .HasOne(ps => ps.ParsedModelResponse)
            .WithMany(p => p.ParsedModelResponseSources)
            .HasForeignKey(ps => ps.ParsedModelResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParsedModelResponseSource>()
            .HasOne(ps => ps.Source)
            .WithMany(s => s.ParsedModelResponseSources)
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExportRow>();
    }
}

