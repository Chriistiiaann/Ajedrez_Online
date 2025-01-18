namespace backEndAjedrez.Models.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}
