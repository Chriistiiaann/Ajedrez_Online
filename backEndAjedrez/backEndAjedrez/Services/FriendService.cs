using backEndAjedrez.Models.Database;
using backEndAjedrez.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace backEndAjedrez.Services;

public class FriendService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FriendService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> SendFriendRequest(string fromUserId, string toUserId)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        if (fromUserId == toUserId) return false; 

        var existingRequest = await _context.FriendRequests
            .FirstOrDefaultAsync(r =>
                (r.FromUserId == fromUserId && r.ToUserId == toUserId) ||
                (r.FromUserId == toUserId && r.ToUserId == fromUserId)
            );

        if (existingRequest != null && existingRequest.Status == "Pending")
        {
            return false;
        }

        var existingFriendship = await _context.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId == fromUserId && f.FriendId == toUserId) ||
                (f.UserId == toUserId && f.FriendId == fromUserId)
            );

        if (existingFriendship != null)
        {
            return false; 
        }

        var request = new FriendRequest
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = "Pending"
        };

        _context.FriendRequests.Add(request);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> AcceptFriendRequest(int requestId)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var request = await _context.FriendRequests.FindAsync(requestId);
        if (request == null || request.Status != "Pending") return false;

        
        request.Status = "Accepted";

        var friend1 = new Friend
        {
            UserId = request.FromUserId,
            FriendId = request.ToUserId
        };
        var friend2 = new Friend
        {
            UserId = request.ToUserId,
            FriendId = request.FromUserId
        };

        _context.Friends.Add(friend1);
        _context.Friends.Add(friend2);
        await _context.SaveChangesAsync();

        // Actualizar solicitud
        await _context.SaveChangesAsync();
        return true;
    }

    // Rechazar solicitud de amistad
    public async Task<bool> RejectFriendRequest(int requestId)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var request = await _context.FriendRequests.FindAsync(requestId);
        if (request == null || request.Status != "Pending") return false;

        request.Status = "Rejected";
        await _context.SaveChangesAsync();
        return true;
    }

    // Obtener solicitudes pendientes
    public async Task<List<FriendRequest>> GetPendingRequests(string userId)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        return await _context.FriendRequests
            .Where(r => r.ToUserId == userId && r.Status == "Pending")
            .ToListAsync();
    }

    // Obtener amigos de un usuario
    public async Task<List<Friend>> GetFriends(string userId)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

        return await _context.Friends
            .Where(f => f.FriendId == userId || f.UserId == userId)
            .ToListAsync();
    }
}
