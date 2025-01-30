using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backEndAjedrez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmartSearchController : ControllerBase
    {
        private readonly SmartSearchService _smartSearchService;
        private readonly IUserRepository _userRepository;

        public SmartSearchController(SmartSearchService smartSearchService, IUserRepository userRepository)
        {
            _smartSearchService = smartSearchService;
            _userRepository = userRepository;
        }

        [HttpPost("Search")]
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


    }
}
