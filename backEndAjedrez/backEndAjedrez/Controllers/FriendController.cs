using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
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
}
