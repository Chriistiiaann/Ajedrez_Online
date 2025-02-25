using backEndAjedrez.Models.Database;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Text;

namespace backEndAjedrez.Services
{
    public class MatchMakingService : IMatchMaking
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Dictionary<string, WebSocket> _connections = new();

        public MatchMakingService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        // ✅ 1. INVITAR A UN AMIGO A UNA PARTIDA
        public async Task<MatchRequest?> InviteFriendAsync(int hostId, string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
            {
                return null;
            }

            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();


            bool areFriends = await _context.Friends
                .AnyAsync(f => (f.UserId == hostId.ToString() && f.FriendId == friendId) ||
                               (f.UserId == friendId && f.FriendId == hostId.ToString()));

            if (!areFriends)
                return null;


            if (int.TryParse(friendId, out var parsedFriendId))
            {
                var match = new MatchRequest
                {
                    HostId = hostId,
                    GuestId = parsedFriendId,
                    Status = "Pending",
                    IsInvitedMatch = true,
                    IsBotGame = false
                };

                _context.MatchRequests.Add(match);
                await _context.SaveChangesAsync();

                await SendMessageToUser(friendId, $"🎮 Has recibido una invitación de {hostId}. Acepta para jugar.");
                await SendMatchUpdateAsync(match.GameId, "Invitación aceptada. Juego iniciado.");
                return match;
            }

            return null;
        }

        // ✅ 2. PARTIDA ALEATORIA (MATCHMAKING AUTOMÁTICO)
        public async Task<MatchRequest> FindOrCreateMatchAsync(int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var pendingMatch = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.Status == "Pending" && (m.GuestId == null || m.GuestId == userId));

            if (pendingMatch != null)
            {
                if (pendingMatch.GuestId == null)
                {
                    // Unirse como guest
                    pendingMatch.GuestId = userId;
                    pendingMatch.Status = "Matched";
                    await _context.SaveChangesAsync();
                    await SendMatchUpdateAsync(pendingMatch.GameId, "🟢 Se ha encontrado un oponente.");
                }
                else if (pendingMatch.GuestId == userId)
                {
                    pendingMatch.Status = "Matched";
                    await _context.SaveChangesAsync();
                    await SendMatchUpdateAsync(pendingMatch.GameId, "🟢 El jugador se ha reconectado.");
                }

                return pendingMatch;
            }

            var newMatch = new MatchRequest
            {
                HostId = userId,
                Status = "Pending"
            };

            _context.MatchRequests.Add(newMatch);
            await _context.SaveChangesAsync();
            return newMatch;
        }

        // ✅ 3. PARTIDA CONTRA BOT
        public async Task<MatchRequest> CreateBotMatchAsync(int hostId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var botMatch = new MatchRequest
            {
                HostId = hostId,
                GuestId = -1, // Indica que el rival es un bot
                IsBotGame = true,
                IsInvitedMatch = false,
                Status = "Matched"
            };

            _context.MatchRequests.Add(botMatch);
            await _context.SaveChangesAsync();
            await SendMatchUpdateAsync(botMatch.GameId, "Partida contra bot iniciada.");
            return botMatch;
        }

        // ✅ 4. ELIMINAR UNA PARTIDA
        public async Task<bool> RemoveRoomAsync(string gameId, int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId);

            if (match == null || (match.HostId != userId && match.GuestId != userId))
                return false;

            _context.MatchRequests.Remove(match);
            await _context.SaveChangesAsync();
            await SendMatchUpdateAsync(gameId, "La sala ha sido eliminada.");
            return true;
        }

        // ✅ 5. OBTENER EL ESTADO DE UNA PARTIDA
        public async Task<MatchRequest?> GetRoomByIdAsync(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId);
        }

        // ✅ 6. WEBSOCKET PARA ACTUALIZACIONES EN TIEMPO REAL
        public async Task SendMatchUpdateAsync(string gameId, string message)
        {
            if (_connections.TryGetValue(gameId, out WebSocket socket) && socket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        // ✅ 7. CONECTAR UN WEBSOCKET A UNA PARTIDA
        public async Task<bool> ConnectToMatchWebSocketAsync(string gameId, WebSocket socket)
        {
            _connections[gameId] = socket;
            var match = await GetRoomByIdAsync(gameId);

            if (match == null)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Partida no encontrada", CancellationToken.None);
                return false;
            }

            await SendMatchUpdateAsync(gameId, "WebSocket conectado.");
            return true;
        }
        // ✅ 8. ACEPTAR UNA PARTIDA
        public async Task<bool> AcceptMatchInvitationAsync(string gameId, int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId && m.GuestId == userId);

            if (match == null || match.Status != "Pending")
            {
                return false;
            }

            match.Status = "Matched";
            await _context.SaveChangesAsync();

            await SendMatchUpdateAsync(match.GameId, "{\"message\": \"Invitación aceptada. Juego iniciado.\"}");
            await SendMessageToUser(match.HostId.ToString(), "{\"message\": \"La invitación a la partida fue aceptada\"}");

            return true;
        }

        // ✅ 8. RECHAZAR UNA PARTIDA
        public async Task<bool> RejectMatchInvitationAsync(string gameId, int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId && m.GuestId == userId);

            if (match == null || match.Status != "Pending")
            {
                return false;
            }

            match.Status = "Rejected";
            await _context.SaveChangesAsync();

            await SendMessageToUser(match.HostId.ToString(), "{\"message\": \"❌ {userId} ha rechazado tu invitación.\"}");
            return true;
        }

        // ✅ 9. ELIMINAR JUGADOR DE LA PARTIDA
        public async Task<bool> RemovePlayerFromMatchAsync(string gameId, int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId);

            if (match == null)
                return false;

            if (match.HostId == userId)
            {
                if (match.GuestId != null)
                {
                    match.HostId = match.GuestId.Value;
                    match.GuestId = null;
                    match.Status = "Pending";
                    await _context.SaveChangesAsync();

                    await SendMessageToUser(match.HostId.ToString(), "{\"message\": \"⚠️ El host ha abandonado. Ahora eres el nuevo host.\"}");
                    return true;
                }
                else
                {
                    _context.MatchRequests.Remove(match);
                    await _context.SaveChangesAsync();
                    await SendMatchUpdateAsync(gameId, "{\"message\": \"❌ La sala ha sido eliminada porque ambos jugadores abandonaron.\"}");
                    return true;
                }
            }
            else if (match.GuestId == userId)
            {
                match.GuestId = null;
                match.Status = "Pending";
                await _context.SaveChangesAsync();
                await SendMessageToUser(match.HostId.ToString(), "{\"message\": \"⚠️ Tu oponente ha abandonado la partida.\"}");
                return true;
            }

            return false;
        }

        // ✅ 10. OBTENER ID DE LA PARTIDA MEDIANTE UN USUARIO   
        public async Task<string?> GetGameIdByUserAsync(string userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            int userIdInt = int.Parse(userId);

            var match = await _context.MatchRequests
                .Where(m => m.HostId == userIdInt || m.GuestId == userIdInt)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            return match?.GameId;
        }

        // ✅ 11. OBTENER ID DEL OPONENTE
        public async Task<string?> GetOpponentIdAsync(string userId, string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            int userIdInt = int.Parse(userId);

            var match = await _context.MatchRequests
                .Where(m => m.GameId == gameId)
                .FirstOrDefaultAsync();

            if (match == null) return null;

            return match.HostId == userIdInt ? match.GuestId?.ToString() : match.HostId.ToString();
        }

        // ✅ 12. VER SI ES UNA PARTIDA CON INVITACIÓN
        public async Task<bool> IsInvitedMatchAsync(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.GameId == gameId);

            if (match == null) return false;

            if (match.IsInvitedMatch)
            {
                match.Status = "Abandoned";
                await _context.SaveChangesAsync();
            }

            return match.IsInvitedMatch;
        }
        public async Task<UserDto?> GetUserInfoAsync(int userId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    NickName = u.NickName,
                    Avatar = u.Avatar
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public string GetHostId(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var hostId = _context.MatchRequests
                .Where(g => g.GameId == gameId)
                .Select(h => h.HostId.ToString())
                .FirstOrDefault();


            return hostId;
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            if (_connections.TryGetValue(userId, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var response = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task<MatchRequest> GetMatchByGameIdAsync(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.GameId == gameId);

            if (match == null)
            {
                throw new Exception($"No se encontró ninguna partida con el GameId: {gameId}");
            }

            return match;
        }

        public async Task UpdateMatchStatusAsync(string gameId, string status)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.GameId == gameId);

            if (match == null)
            {
                throw new Exception($"No se encontró ninguna partida con el GameId: {gameId}");
            }

            match.Status = status;
            _context.MatchRequests.Update(match);
            await _context.SaveChangesAsync();
        }
    }
}