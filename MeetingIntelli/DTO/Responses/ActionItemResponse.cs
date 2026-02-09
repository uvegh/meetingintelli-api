namespace MeetingIntelli.DTO.Responses;

public class ActionItemResponse
{
    public string Assignee { get; set; } = string.Empty;
    public string Task { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = "Medium";
}
