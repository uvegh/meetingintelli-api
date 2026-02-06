namespace MeetingIntelli.DTO.Responses;

public class MeetingResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }
    public string Attendees { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? SUmmary { get; set; }
    public List<ActionItemResponse> ?ActionItems { get; set; }
    public DateTime CreatedAt { get; set; }


}




