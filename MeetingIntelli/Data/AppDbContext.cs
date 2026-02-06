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

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("meetings");

           
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")  // PostgreSQL UUID type
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");  // PostgreSQL generates UUID

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.MeetingDate)
                .HasColumnName("meeting_date")
                .IsRequired();

            entity.Property(e => e.Attendees)
                .HasColumnName("attendees")
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Notes)
                .HasColumnName("notes")
                .IsRequired()
                .HasColumnType("text");

            entity.Property(e => e.Summary)
                .HasColumnName("summary")
                .HasColumnType("text");

            entity.Property(e => e.ActionItemsJson)
                .HasColumnName("action_items_json")
                .HasColumnType("text");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            // Indexes
            entity.HasIndex(e => e.MeetingDate)
                .HasDatabaseName("idx_meeting_date");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_created_at");
        });
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