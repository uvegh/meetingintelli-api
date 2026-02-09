using System.ComponentModel.DataAnnotations;

namespace MeetingIntelli.Models;

public class ActionItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MeetingId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Assignee { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Task { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public Meeting Meeting { get; set; } = null!;
}