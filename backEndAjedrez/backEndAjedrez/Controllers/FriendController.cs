using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Services;
using Microsoft.AspNetCore.Http;
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

    [HttpPost]
    public async Task<IActionResult> GetFriends([FromBody] SearchFriendsDTO id)
    {
        var friends = await _friendRepository.GetFriendsAsync(id.UserId);

        if (friends == null || !friends.Any())
            return NotFound(new { message = "Aún no tienes amigos" });
        

        return Ok(new { friends });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFriend(int userId, int friendId)
    {
        bool deleted = await _friendRepository.DeleteFriendsAsync(userId, friendId);

        if (deleted)
        {
            return Ok(new { message = "Amigo eliminado correctamente" });
        }
        else
        {
            return BadRequest(new { message = "Amigo Inexistente"});
        }
    }

    [HttpGet("pending/{userId}")]
    public async Task<IActionResult> GetPendingRequests(string userId)
    {
        var requests = await _friendRepository.GetPendingRequestsAsync(userId);

        if (requests == null || requests.Count == 0)
        {
            return BadRequest(new { Message = "¡Vaya! Aún no tienes solicitudes de amistad pendientes." });
        }

        // Creamos un objeto que contiene la colección bajo el nombre 'pendingFriendshipRequest'
        var response = new
        {
            pendingFriendshipRequest = requests
        };

        return Ok(response);
    }

}
