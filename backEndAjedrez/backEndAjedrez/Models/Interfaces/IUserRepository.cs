using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Models.Interfaces
{
    public interface IUserRepository
    {

        Task<ICollection<User>> GetUsersAsync();
        Task<IEnumerable<UserDto>> GetUsers(int userId);
        Task<IEnumerable<UserDto>> GetUsers();
        Task<User> GetUserByNickNameAsync(string nickname);
        Task CreateUserAsync(UserCreateDto user);
        Task UpdateUserAsync(UserCreateDto user);
        Task<string> StoreImageAsync(IFormFile file, string modelName);
        Task<User> GetUserByEmailAsync(string email);
        Task<string> NormalizeNickname(string nickname);
        Task<string> GetUserHistory(int userId, int page = 1, int pageSize = 10);
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        Task<UserDto> GetUserByIdAsync(int userId);
    }
}
