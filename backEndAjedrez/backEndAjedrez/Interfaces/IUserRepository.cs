using backEndAjedrez.Models;

namespace backEndAjedrez.Interfaces
{
    public interface IUserRepository
    {

        Task<ICollection<User>> GetUsersAsync();
        Task<User> GetUserByNickNameAsync(string nickname);
        Task CreateUserAsync(User user);
        //Task<User> GetUserByEmailAsync(string email);
        //Task DeleteUserAsync(long id);
        //Task UpdateUserAsync(UserCreateDTO user);
        

    }
}
