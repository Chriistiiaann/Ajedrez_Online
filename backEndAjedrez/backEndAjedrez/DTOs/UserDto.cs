namespace backEndAjedrez.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
    }

    public class UserCreateDto
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}