using System.ComponentModel.DataAnnotations;

namespace MeetingIntelli.DTO.Requests;

public class CreateMeetingRequest
{

    [Required(ErrorMessage = "Titlke is required")]
    [StringLength(200, ErrorMessage = "Title must be less than 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage ="Meeting Date is required")]
    public DateTime MeetingDate { get; set; }

    [Required(ErrorMessage = "At least one attnedee is required")]
    [StringLength(500, ErrorMessage = "Attendees must be less than 500 characters")]

    public string Attendees { get; set; } = string.Empty;

    [Required(ErrorMessage = "Meeting notes are required")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Notes must be between 10 and 5000 characters")]
    public string Notes { get; set; } = string.Empty;

}
