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

    public async Task<bool> RemoveFriend(string userId, string friendId)
    {
        var friendship1 = await _context.Friends
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friendId);
        var friendship2 = await _context.Friends
            .FirstOrDefaultAsync(f => f.UserId == friendId && f.FriendId == userId);

        if (friendship1 == null && friendship2 == null)
        {
            return false;
        }

        if (friendship1 != null)
        {
            _context.Friends.Remove(friendship1);
        }
        if (friendship2 != null)
        {
            _context.Friends.Remove(friendship2);
        }

        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<PendingFriendRequestDto>> GetPendingRequestsAsync(string userId)
    {
        return await _context.FriendRequests
            .Where(r => r.ToUserId == userId && r.Status == "Pending")
            .Join(_context.Users,
                  request => request.FromUserId,
                  user => user.Id.ToString(),
                  (request, user) => new PendingFriendRequestDto
                  {
                      RequestId = request.Id,
                      NickName = user.NickName,
                      Status = user.Status,
                      Avatar = user.Avatar,
                      Timestamp = request.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") // Formato legible
                  })
            .ToListAsync();
    }




}
