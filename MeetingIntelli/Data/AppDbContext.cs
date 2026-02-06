using MeetingIntelli.Configurations;
using MeetingIntelli.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetingIntelli.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Meeting> Meetings => Set<Meeting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MeetingConfiguration());
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Meeting && e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            ((Meeting)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}