using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Database;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Services;

public class StatusService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StatusService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> ChangeStatusAsync(int userId, string newStatus)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        user.Status = newStatus;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> TotalUserConected()
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        return await _context.Users
                             .Where(u => u.Status.Equals("Connected"))
                             .CountAsync();
    }
}