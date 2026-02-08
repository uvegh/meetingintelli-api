using MeetingIntelli.DTO.Common;
using MeetingIntelli.DTO.Responses;
using MeetingIntelli.Interface;
using System.Text.Json;

namespace MeetingIntelli.Services;

public class MeetingAnalysisService : IMeetingAnalysisService
{
    private readonly IClaudeService _claudeService;
    private readonly ILogger<MeetingAnalysisService> _logger;

    public MeetingAnalysisService(
        IClaudeService claudeService,
        ILogger<MeetingAnalysisService> logger)
    {
        _claudeService = claudeService ?? throw new ArgumentNullException(nameof(claudeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MeetingAnalysisResult> AnalyzeMeetingAsync(
        string notes,
        string attendees,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Notes cannot be empty", nameof(notes));
        }

        if (string.IsNullOrWhiteSpace(attendees))
        {
            throw new ArgumentException("Attendees cannot be empty", nameof(attendees));
        }

        try
        {
            _logger.LogInformation("Analyzing meeting notes with AI");

            var prompt = BuildAnalysisPrompt(notes, attendees);
            var response = await _claudeService.GetCompletionAsync(prompt, cancellationToken);
            var result = ParseAiResponse(response);

            _logger.LogInformation(
                "Successfully analyzed meeting. Found {ActionItemCount} action items",
                result.ActionItems.Count
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing meeting notes");

            // Return empty result instead of throwing
            // Business logic: If AI fails, still save the meeting without analysis
            return new MeetingAnalysisResult
            {
                Summary = "AI analysis unavailable",
                ActionItems = new List<ActionItemResponse>()
            };
        }
    }

    private static string BuildAnalysisPrompt(string notes, string attendees)
    {
        return $@"
Analyze these meeting notes and extract structured information.

MEETING NOTES:
{notes}

ATTENDEES: {attendees}

Extract the following and return ONLY valid JSON (no markdown, no explanation):

{{
    ""summary"": ""2-3 sentence overview of what was discussed and decided"",
    ""actionItems"": [
        {{
            ""assignee"": ""person's name from attendees"",
            ""task"": ""specific action they need to take"",
            ""dueDate"": ""YYYY-MM-DD format if mentioned, otherwise null"",
            ""priority"": ""High or Medium or Low based on urgency""
        }}
    ]
}}

Rules:
- Only extract action items where someone needs to DO something
- assignee must be a name from the attendees list
- If no deadline mentioned, set dueDate to null
- Priority High = urgent words like ""ASAP"", ""by Friday"", ""urgent""
- Priority Medium = ""next week"", ""soon"", no urgency indicated
- Priority Low = ""eventually"", ""when you can""
- If no clear action items, return empty array
";
    }

    private MeetingAnalysisResult ParseAiResponse(string response)
    {
        try
        {
            // Remove markdown code blocks if present
            var cleanedResponse = response.Trim();
            if (cleanedResponse.StartsWith("```"))
            {
                var lines = cleanedResponse.Split('\n');
                if (lines.Length > 2)
                {
                    cleanedResponse = string.Join('\n', lines.Skip(1).SkipLast(1));
                }
            }

            cleanedResponse = cleanedResponse.Replace("```json", "").Replace("```", "").Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<MeetingAnalysisResult>(
                cleanedResponse,
                options
            );

            return result ?? new MeetingAnalysisResult();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response: {Response}", response);
            return new MeetingAnalysisResult
            {
                Summary = "Unable to parse AI analysis",
                ActionItems = new List<ActionItemResponse>()
            };
        }
    }


}