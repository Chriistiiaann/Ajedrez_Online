using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Database;
using Microsoft.EntityFrameworkCore;
using backEndAjedrez.Models.Dtos;

namespace backEndAjedrez.Services;

public class StatusService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public StatusService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> ChangeStatusAsync(int userId, string newStatus)
    {
        await _semaphore.WaitAsync();
        bool success = false;
        try
        {
            IServiceScope scope = _serviceScopeFactory.CreateScope();
            DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);


            user.Status = newStatus;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
             success = true;
        }
        catch (Exception ex)
        {

        }
        finally
        {
            _semaphore.Release();
        }

        return success;
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
