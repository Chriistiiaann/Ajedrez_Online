namespace backEndAjedrez.Models.Dtos;

public class MatchHistoryResponse
{
    public List<MatchHistoryItem> History { get; set; }
}

public class MatchHistoryItem
{
    public string GameId { get; set; }
    public string UserName { get; set; }
    public string OpponentName { get; set; }
    public string Winner { get; set; }
    public string MatchDate { get; set; }
}
