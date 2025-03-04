using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backEndAjedrez.Models.Database.Entities;

public class MatchHistory
{
    [Key]
    public int Id { get; set; } 

    public string GameId { get; set; } 

    public int UserId { get; set; } 

    public string UserName { get; set; } 

    public int? OpponentId { get; set; } 

    public string OpponentName { get; set; } 

    public string Winner { get; set; } 

    public DateTime MatchDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("GameId")]
    public virtual MatchRequest MatchRequest { get; set; }
}
