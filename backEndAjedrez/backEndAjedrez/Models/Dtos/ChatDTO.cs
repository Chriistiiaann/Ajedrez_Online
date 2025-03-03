namespace backEndAjedrez.Models.Dtos;

public class ChatDTO
{
    public string GameId { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderAvatar { get; set; }
    public string Message { get; set; }
    public bool IsSender { get; set; }
}