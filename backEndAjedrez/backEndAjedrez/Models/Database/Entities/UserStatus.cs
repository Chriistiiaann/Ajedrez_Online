using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace backEndAjedrez.Models.Database.Entities;

public class UserStatus
{
    [Key, ForeignKey("User")]
    public int UserId { get; set; }
    public string Status { get; set; } // Ej: "Conectado", "Desconectado", "Jugando"
    public DateTime LastUpdated { get; set; } // Última actualización del estado
}
