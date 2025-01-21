using Microsoft.AspNetCore.Mvc;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Mappers;
using backEndAjedrez.Models.Database.Repositories;
using backEndAjedrez.Services;
using Microsoft.AspNetCore.Identity;
using backEndAjedrez.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Database;

namespace backEndAjedrez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly UserMapper _userMapper;
    

        public UserController(UserRepository userRepository, UserMapper userMapper)
        {
            _userRepository = userRepository;
            _userMapper = userMapper;
       
        }


        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var users = await _userRepository.GetUsersAsync();

                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                IEnumerable<UserDto> usersDto = _userMapper.usersToDto(users);

                return Ok(usersDto);
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
                var user = await _userRepository.GetUserByNickNameAsync(nickname);

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
            string normalizedNickname = await _userRepository.NormalizeNickname(userToAddDto.NickName);

            var existingNickname = await _userRepository.GetUserByNickNameAsync(normalizedNickname);
            if (existingNickname != null)
            {
                return Conflict(new
                {
                    message = "Nickname existente, por favor introduzca otro.",
                    code = "NICKNAME_ALREADY_EXISTS"
                });
            }

            string normalizedEmail = await _userRepository.NormalizeNickname(userToAddDto.Email);

            var existingEmail = await _userRepository.GetUserByEmailAsync(normalizedEmail);
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
                await _userRepository.CreateUserAsync(userToAddDto);
                return Ok(new {message = "Usuario registrado con éxito"});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
            }
            
        }
    }
    }

