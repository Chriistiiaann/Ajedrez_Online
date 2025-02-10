using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Models.Interfaces;

public interface IFriendRepository
{
    Task<IEnumerable<UserDto>> GetFriendsAsync(int userId);
    Task<bool> DeleteFriendsAsync(int userId, int friendId);
    Task<List<PendingFriendRequestDto>> GetPendingRequestsAsync(string userId);
}
