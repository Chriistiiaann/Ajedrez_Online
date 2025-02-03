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

        // Primero obtienes todas las relaciones de amigos
        var friends = await _context.Friends
            .Where(f => f.UserId == userIdStr || f.FriendId == userIdStr)
            .Select(f => new
            {
                FriendId = f.UserId == userIdStr ? f.FriendId : f.UserId
            })
            .Distinct()  // Asegura que no se repitan las relaciones
            .ToListAsync();

        // Después haces el join con los usuarios para obtener los detalles de cada amigo
        var friendsDetails = await _context.Users
            .Where(user => friends.Select(f => f.FriendId).Contains(user.Id.ToString()))
            .Select(user => new UserDto
            {
                Id = user.Id,
                NickName = user.NickName,
                Email = user.Email,
                Avatar = user.Avatar
            })
            .ToListAsync();

        return friendsDetails;
    }

}
