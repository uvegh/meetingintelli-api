using MeetingIntelli.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetingIntelli.Configurations;

//public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
//{
//    public void Configure(EntityTypeBuilder<Meeting> builder)
//    {
//        builder.ToTable("meetings");

//        builder.HasKey(e => e.Id);

//        builder.Property(e => e.Id)
//            .HasColumnName("id")
//            .IsRequired();


//        builder.Property(e => e.Title)
//            .HasColumnName("title")
//            .IsRequired()
//            .HasMaxLength(200);

//        builder.Property(e => e.MeetingDate)
//            .HasColumnName("meeting_date")
//            .IsRequired();

//        builder.Property(e => e.Attendees)
//            .HasColumnName("attendees")
//            .IsRequired()
//            .HasMaxLength(500);

//        builder.Property(e => e.Notes)
//            .HasColumnName("notes")
//            .IsRequired();

//        builder.Property(e => e.Summary)
//            .HasColumnName("summary");



//        builder.Property(e => e.CreatedAt)
//            .HasColumnName("created_at")
//            .IsRequired()
//            .HasDefaultValueSql("GETUTCDATE()");

//        builder.Property(e => e.UpdatedAt)
//            .HasColumnName("updated_at");


//        builder.HasMany(e => e.ActionItems)
//            .WithOne(a => a.Meeting)
//            .HasForeignKey(a => a.MeetingId)
//            .OnDelete(DeleteBehavior.Cascade);

//        // Index meetingdate and created at
//        builder.HasIndex(e => e.MeetingDate)
//            .HasDatabaseName("IX_meetings_meeting_date");

//        builder.HasIndex(e => e.CreatedAt)
//            .HasDatabaseName("IX_meetings_created_at");
//    }
//}

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.ToTable("meetings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

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

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");


        // Relationships
        builder.HasMany(e => e.ActionItems)
            .WithOne(a => a.Meeting)
            .HasForeignKey(a => a.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.MeetingDate)
            .HasDatabaseName("IX_meetings_meeting_date");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_meetings_created_at");
    }
}