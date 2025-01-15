﻿using backEndAjedrez.DbContext;
using backEndAjedrez.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backEndAjedrez.Models;
using backEndAjedrez.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backEndAjedrez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenValidationParameters _tokenParameters;
        private readonly DataBaseContext _context;
        private readonly IPasswordHasher _passwordHash;

        public AuthController(IOptionsMonitor<JwtBearerOptions> jwOptions, DataBaseContext context, IPasswordHasher passwordHash)
        {
            _tokenParameters = jwOptions.Get(JwtBearerDefaults.AuthenticationScheme)
                .TokenValidationParameters;
            _context = context;
            _passwordHash = passwordHash;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginModel model)
        {
            string hashedPassword = _passwordHash.Hash(model.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);

            if (user != null)
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = new Dictionary<string, object>
                    {
                        { "Id", user.Id },
                        { "NickName", user.NickName },
                        { "Email", user.Email },
                        
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
