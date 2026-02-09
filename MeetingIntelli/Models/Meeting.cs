using System.ComponentModel.DataAnnotations;

namespace MeetingIntelli.Models;

public class Meeting
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime MeetingDate { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Attendees { get; set; } = string.Empty;

    public string? Summary { get; set; }

  

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    
    public ICollection<ActionItem> ActionItems { get; set; } = new List<ActionItem>();
}