namespace backEndAjedrez.Models.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Avatar {  get; set; }
        public string Status { get; set; }
    }

    public class UserCreateDto
    {
        public int Id { get; set; }
        public string? NickName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public IFormFile? File { get; set; }
        
    }
}