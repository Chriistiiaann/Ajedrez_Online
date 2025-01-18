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
                // Intentar obtener los usuarios desde el repositorio
                var users = await _userRepository.GetUsersAsync();

                // Comprobar si la lista de usuarios es nula o está vacía
                if (users == null || !users.Any())
                {
                    return NotFound("No users found.");
                }

                // Creación del user DTO por cada User en la base de datos
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

            var existingNickname = await _userRepository.GetUserByNickNameAsync(userToAddDto.NickName);
            if (existingNickname != null)
            {
                return Conflict(new
                {
                    message = "Nickname existente, por favor introduzca otro.",
                    code = "NICKNAME_ALREADY_EXISTS"
                });
            }

            var existingEmail = await _userRepository.GetUserByEmailAsync(userToAddDto.Email);
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

