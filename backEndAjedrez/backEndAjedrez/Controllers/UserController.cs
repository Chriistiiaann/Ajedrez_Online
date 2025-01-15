using backEndAjedrez.Repositories;
using backEndAjedrez.DataMappers;
using Microsoft.AspNetCore.Mvc;
using backEndAjedrez.DTOs;
using backEndAjedrez.Models;
using Microsoft.AspNetCore.Identity;

namespace backEndAjedrez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly UserMapper _userMapper;

        public UserController(UserRepository userRepository, UserMapper userMapper) {

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
                // Intentar obtener el usuario desde el repositorio
                var user = await _userRepository.GetUserByNickNameAsync(nickname);

                // Comprobar si el usuario no existe
                if (user == null)
                {
                    return NotFound($"User with Nick Name {nickname} not found.");
                }

                // Crear UserDTO según el User encontrado
                UserDto userDto = _userMapper.ToDto(user);

                return Ok(userDto);
            } 
            catch (Exception ex)
            {
                // Capturar cualquier error inesperado y devolver una respuesta de error 500
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> AddUserAsync([FromBody] UserCreateDto userToAddDto)
        {
            if (userToAddDto == null)
            {
                return BadRequest("Información necesaria no enviada.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userRepository.GetUserByNickNameAsync(userToAddDto.NickName);
            if (existingUser != null)
            {
                return Conflict("Email existente, por favor introduzca otro Email.");
            }

            try
            {
                var userToAdd = new User
                {
                    Id = userToAddDto.Id,
                    NickName = userToAddDto.NickName,
                    Email = userToAddDto.Email,
                    Password = userToAddDto.Password,
                    
                };

                var passwordHasher = new PasswordHasher();
                userToAdd.Password = passwordHasher.Hash(userToAdd.Password);

                await _userRepository.CreateUserAsync(userToAdd);

                return Ok(new { message = "Usuario registrado con éxito." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
    }

