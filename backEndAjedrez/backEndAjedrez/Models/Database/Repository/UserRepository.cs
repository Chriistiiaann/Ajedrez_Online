using backEndAjedrez.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Services;
using System.Globalization;
using System.Text;
using backEndAjedrez.Models.Mappers;

namespace backEndAjedrez.Models.Database.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly DataContext _context;
        private readonly UserMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;

        public UserRepository(DataContext context, UserMapper mapper, IPasswordHasher passwordHasher)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
        }

        public async Task<ICollection<User>> GetUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(user => _mapper.ToDto(user));
        }

        public async Task<IEnumerable<UserDto>> GetUsers(int userId)
        {
            var users = await _context.Users
                .Where(u => u.Id != userId) // Excluir al usuario que realiza la búsqueda
                .OrderBy(u => u.Id)
                .ToListAsync();

            // Mapeamos los usuarios a UserDto
            return users.Select(user => _mapper.ToDto(user));
        }


        public async Task<User> GetUserByNickNameAsync(string nickname)
        {
            string normalizedNickname = await NormalizeNickname(nickname);

            var nicknames = await _context.Users
                .ToListAsync();

            return nicknames.FirstOrDefault(u => NormalizeNickname(u.NickName).Result == normalizedNickname);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            string normalizedEmail = await NormalizeNickname(email);

            var emails = await _context.Users
                .ToListAsync();

            return emails.FirstOrDefault(u => NormalizeNickname(u.NickName).Result == normalizedEmail);
        }

        public async Task<string> NormalizeNickname(string nickname)
        {
            var normalized = await NormalizeIdentifier(nickname);
            return normalized;
        }
        
        public async Task<string> NormalizeIdentifier(string identifier)
        {
            var normalizedIdentifier = identifier.ToLower();
            
            normalizedIdentifier = new string(normalizedIdentifier
                .Normalize(NormalizationForm.FormD)  
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) 
                .ToArray());

            return normalizedIdentifier;
        }

        public async Task<string> StoreImageAsync(IFormFile file, string modelName)
        {
            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };

            if (!validImageTypes.Contains(file.ContentType))
            {
                throw new ArgumentException("El archivo no es un formato de imagen válido.");
            }

            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = modelName + fileExtension;

            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            string filePath = Path.Combine(imagesFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("images", fileName).Replace("\\", "/");
        }

        public async Task CreateUserAsync(UserCreateDto userCreateDto)
        {
            var user = new User
            {
                NickName = userCreateDto.NickName,
                Email = userCreateDto.Email,
                Password = userCreateDto.Password
            };

            var passwordHasher = new PasswordService();
            user.Password = passwordHasher.Hash(userCreateDto.Password);

            if (userCreateDto.File != null)
            {
                try
                {
                    user.Avatar = await StoreImageAsync(userCreateDto.File, userCreateDto.NickName);
                }
                catch(Exception ex) {
                    throw new Exception("Error al guardar la imagen: " + ex.Message);
                }
            }
            else
            {
                user.Avatar = Path.Combine("images", "default.png").Replace("\\", "/");
            }
            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserCreateDto user)
        {
            var usuarioVariado = _context.Users.FirstOrDefault(p => p.Id == user.Id);
            if (usuarioVariado == null)
            {
                throw new Exception("La variación del usuario no existe.");
            }

            // Actualizar NickName solo si se proporciona un valor válido
            if (!string.IsNullOrWhiteSpace(user.NickName) && user.NickName != "string")
            {
                usuarioVariado.NickName = user.NickName;
            }
            else
            {
                user.NickName = usuarioVariado.NickName;
            }

            // Actualizar Email solo si se proporciona un valor válido
            if (!string.IsNullOrWhiteSpace(user.Email) && user.Email != "string")
            {
                usuarioVariado.Email = user.Email;
            }
            else
            {
                user.Email = usuarioVariado.Email;
            }

            // Actualizar Password solo si se proporciona un valor válido
            if (!string.IsNullOrWhiteSpace(user.Password) && user.Password != "string")
            {
                var passwordHashedUpdated = _passwordHasher.Hash(user.Password);
                usuarioVariado.Password = passwordHashedUpdated;
            }
            else
            {
                user.Password = usuarioVariado.Password;
            }

            // Manejar avatar solo si se proporciona un archivo
            if (user.File != null)
            {
                try
                {
                    var userAvatar = !string.IsNullOrWhiteSpace(user.NickName) && user.NickName != "string"
                        ? user.NickName
                        : usuarioVariado.NickName;

                    usuarioVariado.Avatar = await StoreImageAsync(user.File, userAvatar);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error al guardar la imagen.", ex);
                }
            }
            else
            {
                // Mantener el avatar existente
                usuarioVariado.Avatar = usuarioVariado.Avatar;
            }

            _context.Users.Update(usuarioVariado);
            await _context.SaveChangesAsync();
        }
    }
}

