using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Models.Interfaces;

public interface IFriendRepository
{
    Task<IEnumerable<UserDto>> GetFriendsAsync(int userId);
    Task<bool> RemoveFriend(string userId, string friendId);
    Task<List<PendingFriendRequestDto>> GetPendingRequestsAsync(string userId);
}
