
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetingIntelli.Configurations;

public class ActionItemConfiguration : IEntityTypeConfiguration<ActionItem>
{
    public void Configure(EntityTypeBuilder<ActionItem> builder)
    {
        builder.ToTable("action_items");

        builder.HasKey(e => e.Id);


        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();
           
        builder.Property(e => e.MeetingId)
            .HasColumnName("meeting_id")
            .IsRequired();

        builder.Property(e => e.Assignee)
            .HasColumnName("assignee")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Task)
            .HasColumnName("task")
            .IsRequired()
            .HasMaxLength(500);



        builder.Property(e => e.DueDate)
            .HasColumnName("due_date");

        builder.Property(e => e.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Medium");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

     
        builder.HasOne(e => e.Meeting)
            .WithMany(m => m.ActionItems)
            .HasForeignKey(e => e.MeetingId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_action_items_meetings");
//create index for  meetingid,duedate and priority
        builder.HasIndex(e => e.MeetingId)
            .HasDatabaseName("IX_action_items_meeting_id");

        builder.HasIndex(e => e.DueDate)
            .HasDatabaseName("IX_action_items_due_date");

        builder.HasIndex(e => e.Priority)
            .HasDatabaseName("IX_action_items_priority");

        builder.HasIndex(e => e.Assignee)
            .HasDatabaseName("IX_action_items_assignee");
    }
}