namespace backEndAjedrez.Models.Dtos;
public class PendingFriendRequestDto
{
    public int RequestId { get; set; }
    public string NickName { get; set; }
    public string Status { get; set; }
    public string? Avatar { get; set; }
    public string Timestamp { get; set; }
}


