namespace MeetingIntelli.DTO.Common;

using MeetingIntelli.DTO.Responses;

public class MeetingAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<ActionItemResponse> ActionItems { get; set; } = new();
}