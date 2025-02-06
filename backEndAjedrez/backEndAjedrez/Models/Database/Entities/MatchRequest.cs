using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace backEndAjedrez.Models.Database.Entities;

public class MatchRequest
{
    [Key]
    public string GameId { get; set; } = Guid.NewGuid().ToString();

    public int HostId { get; set; } 

    public int? GuestId { get; set; } 

    public string Status { get; set; } = "Pending"; 

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

