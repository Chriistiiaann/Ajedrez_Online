using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace backEndAjedrez.Models.Database.Entities;

public class FriendRequest
{
    public int Id { get; set; }
    public string FromUserId { get; set; }
    public string ToUserId { get; set; }
    public string Status { get; set; } = "Pending"; // Estado por defecto
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
