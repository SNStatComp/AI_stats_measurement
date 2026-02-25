using AI_stats_measurement.Backend.Models;
using AI_stats_measurement.Models;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PromptDimension>()
            .HasIndex(d => new { d.PromptId, d.Name })
            .IsUnique();

        modelBuilder.Entity<Prompt>()
            .HasMany(p => p.Dimensions)
            .WithOne()
            .HasForeignKey(d => d.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

