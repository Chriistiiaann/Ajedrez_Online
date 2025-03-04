using System.ComponentModel.DataAnnotations;

namespace backEndAjedrez.Models.Database.Entities;

public class User
{
    public int Id { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Avatar { get; set; }
    public string Status { get; set; } = "Disconnected";
    public string? Role { get; set; } = "None";

}
