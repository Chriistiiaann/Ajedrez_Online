using backEndAjedrez.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;

namespace backEndAjedrez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenValidationParameters _tokenParameters;
        private readonly DataContext _context;
        private readonly IPasswordHasher _passwordHash;

        public AuthController(IOptionsMonitor<JwtBearerOptions> jwOptions, DataContext context, IPasswordHasher passwordHash)
        {
            _tokenParameters = jwOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
            _context = context;
            _passwordHash = passwordHash;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest model)
        {
            string hashedPassword = _passwordHash.Hash(model.Password);

            var user = await _context.Users
    .FirstOrDefaultAsync(u => (u.Email == model.User || u.NickName == model.User) && u.Password == hashedPassword);


            Console.WriteLine($"Login attempt with User: {model.User} and Password: {model.Password}");

            if (user != null)
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = new Dictionary<string, object>
                    {
                        { "Id", user.Id },
                        { "NickName", user.NickName },
                        { "Email", user.Email },
                        { "Avatar", user.Avatar}
                        
                    },
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(
                        _tokenParameters.IssuerSigningKey,
                        SecurityAlgorithms.HmacSha256Signature)
                };

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                string stringToken = tokenHandler.WriteToken(token);

                return Ok(new LoginResult { AccessToken = stringToken });
            }
            else
            {
                return Unauthorized("Email o contraseña incorrecto");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("secret")]
        public ActionResult GetSecret()
        {
            // Si el usuario es admin, devuelve el secreto
            return Ok("Esto es un secreto que no todo el mundo debería leer");
        }
    }
}
