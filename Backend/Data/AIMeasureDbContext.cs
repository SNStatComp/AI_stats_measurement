using Microsoft.EntityFrameworkCore;

namespace AI_stats_measurement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Prompt> Prompts => Set<Prompt>();
}

