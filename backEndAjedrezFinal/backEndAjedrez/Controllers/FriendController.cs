using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backEndAjedrez.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendController : ControllerBase
{
    private readonly IFriendRepository _friendRepository;

    public FriendController(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetFriends(int userId)
    {
        var friends = await _friendRepository.GetFriendsAsync(userId);

        if (friends == null || !friends.Any())
            return NotFound(new { message = "Aún no tienes amigos" });
        

        return Ok(new { friends });
    }

    [HttpDelete("{friendId}")]
    public async Task<IActionResult> DeleteFriend([FromRoute] int friendId, [FromQuery] int userId)
    {
        bool deleted = await _friendRepository.RemoveFriend(userId.ToString(), friendId.ToString());

        if (deleted)
        {
            return Ok(new { message = "Amigo eliminado correctamente" });
        }
        else
        {
            return Ok(new { message = "Amigo inexistente" });
        }
    }

    [HttpGet("pending/{userId}")]
    public async Task<IActionResult> GetPendingRequests(string userId)
    {
        var requests = await _friendRepository.GetPendingRequestsAsync(userId);

        if (requests == null || requests.Count == 0)
        {
            return Ok(new { Message = "¡Vaya! Aún no tienes solicitudes de amistad pendientes." });
        }

        var response = new
        {
            pendingFriendshipRequest = requests
        };

        return Ok(response);
    }

}
