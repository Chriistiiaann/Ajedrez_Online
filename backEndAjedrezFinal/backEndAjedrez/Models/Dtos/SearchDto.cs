namespace backEndAjedrez.Models.Dtos;

public class PorfileSearch
{
    public string Query { get; set; } = "";
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}

public class PeopleSearch
{
    public string Query { get; set; } = "";   
    public int UserId { get; set; } 
}
