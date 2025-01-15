namespace backEndAjedrez.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}
