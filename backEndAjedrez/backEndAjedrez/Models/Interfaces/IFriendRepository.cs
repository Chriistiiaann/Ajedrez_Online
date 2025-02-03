using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Models.Interfaces;

public interface IFriendRepository
{
    Task<IEnumerable<UserDto>> GetFriendsAsync(int userId);
}
