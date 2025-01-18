using backEndAjedrez.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Interfaces;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Services;

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
            return await _context.Users.FirstOrDefaultAsync(u => u.NickName == nickname);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<string> StoreImageAsync(IFormFile file, string modelName)
        {
            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = modelName + fileExtension;

            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            string filePath = Path.Combine(imagesFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
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

                // Agregar el usuario al contexto
                await _context.Users.AddAsync(user);

                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();
            }
            
        }
    }
}

