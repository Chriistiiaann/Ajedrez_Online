using backEndAjedrez.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Services;
using System.Globalization;
using System.Text;

namespace backEndAjedrez.Models.Database.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ICollection<User>> GetUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<User> GetUserByNickNameAsync(string nickname)
        {
            string normalizedNickname = await NormalizeNickname(nickname);
            return await _context.Users.FirstOrDefaultAsync(u => u.NickName == normalizedNickname);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            string normalizedEmail = await NormalizeNickname(email);
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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
    }
}

