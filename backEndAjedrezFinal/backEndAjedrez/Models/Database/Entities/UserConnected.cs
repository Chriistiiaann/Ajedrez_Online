using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace backEndAjedrez.Models.Database.Entities;

public class UserConnected
{
    [Key]
    public int Id { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public DateTime ConnectedSince { get; set; } // Fecha/hora en la que se conectó
    public int? CurrentGameId { get; set; } // Id de la partida si está jugando, null si no
}
