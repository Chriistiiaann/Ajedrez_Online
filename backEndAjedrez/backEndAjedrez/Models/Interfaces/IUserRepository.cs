using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Models.Interfaces
{
    public interface IUserRepository
    {

        Task<ICollection<User>> GetUsersAsync();
        Task<User> GetUserByNickNameAsync(string nickname);
        Task CreateUserAsync(UserCreateDto user);
        Task<string> StoreImageAsync(IFormFile file, string modelName);
        Task<User> GetUserByEmailAsync(string email);
        Task<string> NormalizeNickname(string nickname);
        //Task DeleteUserAsync(long id);
        //Task UpdateUserAsync(UserCreateDTO user);


    }
}
