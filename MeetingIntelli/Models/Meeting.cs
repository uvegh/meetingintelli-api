using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace MeetingIntelli.Models;

public class Meeting
{

    [Key]
    public Guid Id { get; set; } 

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime MeetingDate { get; set; }

    [Required]
    public string Notes { get; set; } = string.Empty;
    public string Attendees { get; set; } = string.Empty;
    public string? Summary { get; set; }

    public string?  ActionItemsJson { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

        
}
