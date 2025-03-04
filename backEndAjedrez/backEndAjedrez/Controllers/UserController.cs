using Microsoft.AspNetCore.Mvc;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Mappers;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Services;
using System.Text.Json;
using backEndAjedrez.Models.Database.Repositories;
using Microsoft.AspNetCore.Authorization;

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
            return Ok(new { message = "Usuario registrado con éxito" });
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

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetUserHistory(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var historyJson = await _userIRepository.GetUserHistory(userId, page, pageSize);
        var historyData = JsonSerializer.Deserialize<dynamic>(historyJson); 

        if (historyData.GetProperty("history").GetArrayLength() == 0)
        {
            return Ok(new { message = "El usuario no ha jugado aún ninguna partida." });
        }

        return Ok(historyJson);
    }

    [HttpPut("update-role")]
    public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserDTO request)
    {
        bool success = await _userIRepository.UpdateUserRoleAsync(request.UserId, request.NewRole);

        if (!success)
        {
            return BadRequest("Error al actualizar el rol o rol inválido.");
        }

        return Ok(new { message = "Rol actualizado correctamente." });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        var user = await _userIRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(user);
    }
}


