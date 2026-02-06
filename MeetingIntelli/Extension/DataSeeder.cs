


namespace MeetingIntelli.Extension;

public static class DatabaseSeeder
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Check if data already exists
            if (await context.Meetings.AnyAsync())
            {
                logger.LogInformation("Database already contains data, skipping seed");
                return;
            }

            logger.LogInformation("Seeding database with test data");

            var meetings = new[]
            {
                new Meeting
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    Title = "Q1 Planning Meeting",
                    MeetingDate = new DateTime(2024, 1, 15, 10, 0, 0),
                    Attendees = "John Smith, Sarah Johnson, Mike Chen",
                    Notes = @"We discussed Q1 targets and objectives. Sarah mentioned we need to increase revenue by 20%. 
John will prepare the financial forecast by end of week. Mike raised concerns about staffing levels.
Action: Sarah to send proposal to clients by Friday. John to call major clients next Tuesday. 
Mike to hire 2 new developers by end of month.",
                    Summary = "Team discussed Q1 targets with focus on 20% revenue increase. Key staffing concerns raised.",
                    ActionItemsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Assignee = "Sarah Johnson", Task = "Send proposal to clients", DueDate = "2024-01-19", Priority = "High" },
                        new { Assignee = "John Smith", Task = "Call major clients", DueDate = "2024-01-23", Priority = "Medium" },
                        new { Assignee = "Mike Chen", Task = "Hire 2 new developers", DueDate = "2024-01-31", Priority = "High" }
                    }),
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },

                new Meeting
                {
                    Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                    Title = "Product Roadmap Review",
                    MeetingDate = new DateTime(2024, 1, 22, 14, 0, 0),
                    Attendees = "Alice Brown, David Lee, Emma Wilson",
                    Notes = @"Reviewed product roadmap for next 6 months. Alice presented new feature ideas.
David suggested we prioritize mobile app improvements. Emma agreed to conduct user research.
Decision: Move AI features to Q2. Alice to create detailed specs. David to estimate development time.
Emma to present research findings at next meeting.",
                    Summary = "Reviewed 6-month product roadmap. AI features moved to Q2, mobile app prioritized.",
                    ActionItemsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Assignee = "Alice Brown", Task = "Create detailed feature specs", DueDate = "2024-01-29", Priority = "High" },
                        new { Assignee = "David Lee", Task = "Estimate development time for mobile improvements", DueDate = "2024-02-05", Priority = "Medium" },
                        new { Assignee = "Emma Wilson", Task = "Conduct user research and present findings", DueDate = "2024-02-12", Priority = "Medium" }
                    }),
                    CreatedAt = DateTime.UtcNow.AddDays(-23)
                },

                new Meeting
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    Title = "Security Audit Debrief",
                    MeetingDate = new DateTime(2024, 1, 29, 11, 0, 0),
                    Attendees = "Tom Anderson, Lisa Martinez, Kevin Park",
                    Notes = @"Discussed findings from security audit. Critical vulnerabilities found in authentication system.
Tom outlined remediation plan. Lisa to patch vulnerabilities ASAP. Kevin to update documentation.
Timeline: All critical issues must be resolved by end of week. Tom to report to management.
Next audit scheduled for March.",
                    Summary = "Security audit revealed critical authentication vulnerabilities requiring immediate remediation.",
                    ActionItemsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Assignee = "Lisa Martinez", Task = "Patch authentication vulnerabilities", DueDate = "2024-02-02", Priority = "High" },
                        new { Assignee = "Kevin Park", Task = "Update security documentation", DueDate = "2024-02-05", Priority = "Medium" },
                        new { Assignee = "Tom Anderson", Task = "Report findings to management", DueDate = "2024-02-01", Priority = "High" }
                    }),
                    CreatedAt = DateTime.UtcNow.AddDays(-16)
                },

                new Meeting
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-4718-9192-a3b4c5d6e7f8"),
                    Title = "Customer Feedback Session",
                    MeetingDate = new DateTime(2024, 2, 2, 15, 30, 0),
                    Attendees = "Rachel Green, Mark Taylor, Nina Patel",
                    Notes = @"Reviewed customer feedback from January. Overall satisfaction score: 4.2/5.
Main complaints: slow loading times, confusing navigation. Rachel to prioritize performance improvements.
Mark to redesign navigation. Nina to follow up with unhappy customers.
Positive feedback: Great customer support, useful features. Keep investing in support team.",
                    Summary = "Customer satisfaction at 4.2/5. Main issues: performance and navigation. Positive feedback on support.",
                    ActionItemsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Assignee = "Rachel Green", Task = "Prioritize performance improvements", DueDate = "2024-02-09", Priority = "High" },
                        new { Assignee = "Mark Taylor", Task = "Redesign navigation interface", DueDate = "2024-02-16", Priority = "Medium" },
                        new { Assignee = "Nina Patel", Task = "Follow up with unhappy customers", DueDate = "2024-02-06", Priority = "High" }
                    }),
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },

                new Meeting
                {
                    Id = Guid.Parse("b2c3d4e5-f6a7-4829-a1b2-c3d4e5f6a7b8"),
                    Title = "Team Retrospective",
                    MeetingDate = new DateTime(2024, 2, 5, 16, 0, 0),
                    Attendees = "All Team Members",
                    Notes = @"Monthly retrospective. What went well: Launched new feature on time, good collaboration.
What didn't go well: Too many meetings, unclear requirements. Action items: Reduce meeting frequency.
Product team to provide clearer specs. Engineering to push back on unclear requirements.
Next retrospective: First Friday of March.",
                    Summary = "Team retrospective identified meeting overload and unclear requirements as main pain points.",
                    ActionItemsJson = JsonSerializer.Serialize(new[]
                    {
                        new { Assignee = "All Team Members", Task = "Reduce meeting frequency and duration", DueDate = (string?)null, Priority = "Low" },
                        new { Assignee = "Product Team", Task = "Provide clearer requirement specifications", DueDate = (string?)null, Priority = "Medium" },
                        new { Assignee = "Engineering Team", Task = "Push back on unclear requirements", DueDate = (string?)null, Priority = "Low" }
                    }),
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                }
            };

            context.Meetings.AddRange(meetings);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully seeded {Count} meetings", meetings.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}