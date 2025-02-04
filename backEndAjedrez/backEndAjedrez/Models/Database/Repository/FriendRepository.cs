using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backEndAjedrez.Models.Database.Repository;

public class FriendRepository : IFriendRepository
{
    private readonly DataContext _context;

    public FriendRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetFriendsAsync(int userId)
    {
        string userIdStr = userId.ToString();

        var friends = await _context.Friends
            .Where(f => f.UserId == userIdStr || f.FriendId == userIdStr)
            .Select(f => new
            {
                FriendId = f.UserId == userIdStr ? f.FriendId : f.UserId
            })
            .Distinct()  
            .ToListAsync();

        var friendsDetails = await _context.Users
            .Where(user => friends.Select(f => f.FriendId).Contains(user.Id.ToString()))
            .Select(user => new UserDto
            {
                Id = user.Id,
                NickName = user.NickName,
                Email = user.Email,
                Avatar = user.Avatar,
                Status = user.Status
            })
            .ToListAsync();

        return friendsDetails;
    }

    public async Task<bool> DeleteFriendsAsync(int userId, int friendId)
    {
        string userIdStr = userId.ToString();
        string friendIdStr = friendId.ToString();

        var friendship = await _context.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId == userIdStr && f.FriendId == friendIdStr) ||
                (f.UserId == friendIdStr && f.FriendId == userIdStr));

        if (friendship != null)
        {
            _context.Friends.Remove(friendship);
            await _context.SaveChangesAsync();
            return true; // Indica que se eliminó correctamente
        }

        return false; // Indica que la amistad no existe
    }


}
