using Microsoft.AspNetCore.Mvc;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Mappers;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;

namespace backEndAjedrez.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserRepository _userIRepository;
    private readonly UserMapper _userMapper;
    private readonly DataContext _dataContext;

    public UserController(IUserRepository userIRepository, UserMapper userMapper, DataContext dataContext)
    {
        _userIRepository = userIRepository;
        _userMapper = userMapper;
        _dataContext = dataContext;
    }


    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var users = await _userIRepository.GetUsers();

            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }
        catch (Exception ex)
        {
            // Captura cualquier error inesperado y devuelve una respuesta de error 500
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("{nickname}")]
    public async Task<IActionResult> GetUserByNickNameAsync(string nickname)
    {
        if (nickname == "")
        {
            return BadRequest("Invalid user Nick Name.");
        }

        try
        {
            var user = await _userIRepository.GetUserByNickNameAsync(nickname);

            if (user == null)
            {
                return NotFound($"User with Nick Name {nickname} not found.");
            }

            UserDto userDto = _userMapper.ToDto(user);

            return Ok(userDto);
        } 
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> AddUserAsync([FromForm] UserCreateDto userToAddDto)
    {
        if (userToAddDto == null)
        {
            return BadRequest(new
            {
                message = "Información necesaria no enviada.",
                code = "MISSING_REQUIRED_INFORMATION"
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        string normalizedNickname = await _userIRepository.NormalizeNickname(userToAddDto.NickName);

        var existingNickname = await _userIRepository.GetUserByNickNameAsync(normalizedNickname);
        if (existingNickname != null)
        {
            return Conflict(new
            {
                message = "Nickname existente, por favor introduzca otro.",
                code = "NICKNAME_ALREADY_EXISTS"
            });
        }

        string normalizedEmail = await _userIRepository.NormalizeNickname(userToAddDto.Email);

        var existingEmail = await _userIRepository.GetUserByEmailAsync(normalizedEmail);
        if (existingEmail != null)
        {
            return Conflict(new
            {
                message = "Email existente, por favor introduzca otro.",
                code = "EMAIL_ALREADY_EXISTS"
            });
        }

        try
        {
            await _userIRepository.CreateUserAsync(userToAddDto);
            return Ok(new {message = "Usuario registrado con éxito"});
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
        }
        
    }
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUserAsync([FromForm] UserCreateDto userToAddDto)
    {
        if (userToAddDto == null)
        {
            return BadRequest(new
            {
                message = "Información necesaria no enviada.",
                code = "MISSING_REQUIRED_INFORMATION"
            });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (userToAddDto.NickName != null)
        {
           string normalizedNickname = await _userIRepository.NormalizeNickname(userToAddDto.NickName);

            var existingNickname = await _userIRepository.GetUserByNickNameAsync(normalizedNickname);
            if (existingNickname != null)
            {
                return Conflict(new
                {
                    message = "Nickname existente, por favor introduzca otro.",
                    code = "NICKNAME_ALREADY_EXISTS"
                });
            } 
        }

        if (userToAddDto.Email != null)
        {
            string normalizedEmail = await _userIRepository.NormalizeNickname(userToAddDto.Email);

            var existingEmail = await _userIRepository.GetUserByEmailAsync(normalizedEmail);
            if (existingEmail != null)
            {
                return Conflict(new
                {
                    message = "Email existente, por favor introduzca otro.",
                    code = "EMAIL_ALREADY_EXISTS"
                });
            }
        }

        try
        {
            await _userIRepository.UpdateUserAsync(userToAddDto);
            return Ok(new { message = "Usuario actualizado con éxito" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
        }

    }
}


