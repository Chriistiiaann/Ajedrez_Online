using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Database;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Services;

public class StatusService
{
    private readonly DataContext _context;

    public StatusService(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> ChangeStatusAsync(int userId, string newStatus)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false; 

        user.Status = newStatus;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> TotalUserConected()
    {
        return await _context.Users
                         .Where(u => u.Status.Equals("Connected"))
                         .CountAsync();
    }
}
