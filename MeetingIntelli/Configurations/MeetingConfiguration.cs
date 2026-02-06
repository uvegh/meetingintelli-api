using MeetingIntelli.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetingIntelli.Configurations
{
    public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {

            builder.ToTable("meetings");

            builder.HasKey(e => e.Id);

           
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired()
                .HasDefaultValueSql("NEWID()");  

            builder.Property(e => e.Title)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.MeetingDate)
                .HasColumnName("meeting_date")
                .IsRequired();

            builder.Property(e => e.Attendees)
                .HasColumnName("attendees")
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.Notes)
                .HasColumnName("notes")
                .IsRequired();

            builder.Property(e => e.Summary)
                .HasColumnName("summary");

            builder.Property(e => e.ActionItemsJson)
                .HasColumnName("action_items_json");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");  // ✅ SQL Server UTC function

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            // Indexes
            builder.HasIndex(e => e.MeetingDate)
                .HasDatabaseName("IX_meetings_meeting_date");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_meetings_created_at");
        }
    }
}
