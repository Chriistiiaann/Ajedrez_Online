using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backEndAjedrez.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SmartSearchController : ControllerBase
    {
        private readonly SmartSearchService _smartSearchService;
        private readonly IUserRepository _userRepository;
        private readonly IFriendRepository _friendRepository;

        public SmartSearchController(SmartSearchService smartSearchService, IUserRepository userRepository, IFriendRepository friendRepository)
        {
            _smartSearchService = smartSearchService;
            _userRepository = userRepository;
            _friendRepository = friendRepository;
        }

        [HttpPost("SearchUsers")]
        public async Task<IActionResult> SearchAsync([FromBody] PeopleSearch request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid request data.");
                }

                if (request.UserId <= 0) // Validación extra para evitar valores incorrectos
                {
                    return BadRequest("Invalid user ID.");
                }

                IEnumerable<UserDto> users = string.IsNullOrWhiteSpace(request.Query)
                    ? await _userRepository.GetUsers(request.UserId)
                    : await _smartSearchService.SearchAsync(request.UserId, request.Query);

                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                return Ok(new { users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("SearchFriends")]
        public async Task<IActionResult> SearchFriendsAsync([FromBody] PeopleSearch request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid request data.");
                }

                if (request.UserId <= 0) 
                {
                    return BadRequest("Invalid user ID.");
                }

                IEnumerable<UserDto> users = string.IsNullOrWhiteSpace(request.Query)
                    ? await _friendRepository.GetFriendsAsync(request.UserId)
                    :  await _smartSearchService.SearchAsync(request.UserId, request.Query);

                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                return Ok(new { users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
