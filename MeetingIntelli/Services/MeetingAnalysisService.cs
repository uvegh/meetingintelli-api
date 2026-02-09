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
                "Successfully analyzed meeting. raw results {response} action items",
                response
            );
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
You are an expert at analyzing meeting notes and extracting action items.

MEETING NOTES:
{notes}

ATTENDEES: {attendees}

Your task:
1. Generate a 2-3 sentence summary of what was discussed and decided
2. Extract EVERY action item, even implicit ones
3. If duedate is 

Look for patterns like:
- ""[Name] will/should/needs to [action]""
- ""[Name] to [action]""
- ""[Name] is responsible for [action]""
- ""We need [Name] to [action]""
- ""Action: [action]"" (infer assignee from context)

Return ONLY valid JSON (no markdown, no backticks, no explanation):

{{
    ""summary"": ""2-3 sentence overview"",
    ""actionItems"": [
        {{
            ""assignee"": ""exact name from attendees list"",
            ""task"": ""clear, specific action"",
            ""dueDate"": ""YYYY-MM-DD or null"",
            ""priority"": ""High or Medium or Low""
        }}
    ]
}}

Priority rules:
- High: ""ASAP"", ""urgent"", ""immediately"", ""by Friday"", ""end of week""
- Medium: ""next week"", ""soon"", ""by end of month"", no urgency mentioned
- Low: ""eventually"", ""when possible"", ""nice to have""

Date extraction examples:
- ""by Friday"" → calculate next Friday's date
- ""end of week"" → calculate this Friday's date
- ""next Tuesday"" → calculate next Tuesday's date
- ""by Jan 15"" → ""2025-01-15""
- no date mentioned → don't return date field

IMPORTANT: Be aggressive - if someone is mentioned doing something, extract it as an action item.
If no assignee is clear, use ""Team"" or infer from context.
If notes don't have ANY actions, return empty actionItems array.
";
    }
    //private MeetingAnalysisResult ParseAiResponse(string response)
    //{
    //    _logger.LogCritical("CLaude raw response {response}", response);
    //    try
    //    {
    //        // Remove markdown code blocks if present
    //        var cleanedResponse = response.Trim();
    //        if (cleanedResponse.StartsWith("```"))
    //        {
    //            var lines = cleanedResponse.Split('\n');
    //            if (lines.Length > 2)
    //            {
    //                cleanedResponse = string.Join('\n', lines.Skip(1).SkipLast(1));
    //            }
    //        }

    //        cleanedResponse = cleanedResponse.Replace("```json", "").Replace("```", "").Trim();

    //        var options = new JsonSerializerOptions
    //        {
    //            PropertyNameCaseInsensitive = true
    //        };

    //        var result = JsonSerializer.Deserialize<MeetingAnalysisResult>(
    //            cleanedResponse,
    //            options
    //        );
    //        _logger.LogCritical("CLaude raw response {response}", cleanedResponse);
    //        return result ?? new MeetingAnalysisResult();
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogWarning(ex, "Failed to parse AI response: {Response}", response);
    //        return new MeetingAnalysisResult
    //        {
    //            Summary = "Unable to parse AI analysis",
    //            ActionItems = new List<ActionItemResponse>()
    //        };
    //    }
    //}

    private MeetingAnalysisResult ParseAiResponse(string response)
    {
        try
        {
            _logger.LogDebug("Raw Claude response: {Response}", response);

            // Step 1: Clean markdown code blocks
            var cleanedResponse = CleanJsonResponse(response);

            _logger.LogDebug("Cleaned response: {CleanedResponse}", cleanedResponse);

            // Step 2: Validate it's valid JSON
            if (!IsValidJson(cleanedResponse))
            {
                _logger.LogWarning("Response is not valid JSON after cleaning");
                return CreateFallbackResult("Invalid JSON format");
            }

            // Step 3: Deserialize with case-insensitive options
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<MeetingAnalysisResult>(
                cleanedResponse,
                options
            );

            // Step 4: Validate deserialization succeeded
            if (result == null)
            {
                _logger.LogWarning("Deserialization returned null");
                return CreateFallbackResult("Deserialization failed");
            }

            // Step 5: Ensure lists are initialized (avoid null reference errors)
            result.ActionItems ??= new List<ActionItemResponse>();

            _logger.LogInformation(
                "Successfully parsed AI response: {Summary}, {ActionItemCount} action items",
                result.Summary,
                result.ActionItems.Count
            );

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "JSON parsing error. Response: {Response}",
                response
            );

            return CreateFallbackResult("JSON parsing error: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error parsing AI response. Response: {Response}",
                response
            );

            return CreateFallbackResult("Unexpected parsing error");
        }
    }

    private static string CleanJsonResponse(string response)
    {
        var cleaned = response.Trim();

        // Remove markdown code blocks (```json ... ``` or ``` ... ```)
        if (cleaned.StartsWith("```"))
        {
            // Find the first newline after ```
            var firstNewline = cleaned.IndexOf('\n');
            if (firstNewline > 0)
            {
                cleaned = cleaned[(firstNewline + 1)..];
            }

            // Remove trailing ```
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned[..^3];
            }
        }

        // Remove any remaining markdown artifacts
        cleaned = cleaned
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
      

        return cleaned;
    }

    private static bool IsValidJson(string text)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static MeetingAnalysisResult CreateFallbackResult(string reason)
    {
        return new MeetingAnalysisResult
        {
            Summary = $"Unable to parse AI analysis: {reason}",
            ActionItems = new List<ActionItemResponse>()
        };
    }
}