using backEndAjedrez.DbContext;
using backEndAjedrez.Interfaces;
using backEndAjedrez.Models;
using backEndAjedrez.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backEndAjedrez.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly DataBaseContext _context;

        public UserRepository(DataBaseContext context)
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

        public async Task CreateUserAsync(User user)
        {
            // Verificar si ya existe un usuario con el mismo nickname
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.NickName == user.NickName);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with the same nickname already exists.");
            }

            // Agregar el usuario al contexto
            await _context.Users.AddAsync(user);

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            
        }
    }
}
