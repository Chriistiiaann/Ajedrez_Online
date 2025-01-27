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

                IEnumerable<UserDto> users = string.IsNullOrWhiteSpace(request.Query)
                    ? await _userRepository.GetUsers()
                    : await _smartSearchService.SearchAsync(request.Query);

                // Validar si no se encontraron resultados
                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                // Formatear el resultado
                var result = new
                {
                    users = users
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Manejo de errores y devolver código 500
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
