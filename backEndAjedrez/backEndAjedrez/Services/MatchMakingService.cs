using backEndAjedrez.Chess_Game;
using backEndAjedrez.Models.Database;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

        public async Task<MatchRequest?> GetRoomByIdAsync(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            return await _context.MatchRequests.FirstOrDefaultAsync(m => m.GameId == gameId);
        }

        public async Task SendMatchUpdateAsync(string gameId, string message)
        {
            if (_connections.TryGetValue(gameId, out WebSocket socket) && socket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

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

        public async Task SaveMatchHistoryAsync(string gameId, string winnerColor)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var match = await _context.MatchRequests
                .FirstOrDefaultAsync(m => m.GameId == gameId);
            if (match == null)
            {
                throw new Exception($"No se encontró la partida con GameId: {gameId}");
            }

            var hostInfo = await GetUserInfoAsync(match.HostId);
            var guestInfo = match.GuestId.HasValue ? await GetUserInfoAsync(match.GuestId.Value) : null;

            string hostName = hostInfo.NickName;
            string guestName = match.IsBotGame ? "Bot" : guestInfo?.NickName;

            bool hostIsWhite = true;
            string winnerName = winnerColor == "White" ? hostName : guestName;

            var hostHistory = new MatchHistory
            {
                GameId = gameId,
                UserId = match.HostId,
                UserName = hostName,
                OpponentId = match.GuestId,
                OpponentName = guestName,
                Winner = winnerName,
                MatchDate = DateTime.UtcNow
            };
            _context.MatchHistory.Add(hostHistory);

            if (match.GuestId.HasValue && !match.IsBotGame)
            {
                var guestHistory = new MatchHistory
                {
                    GameId = gameId,
                    UserId = match.GuestId.Value,
                    UserName = guestName,
                    OpponentId = match.HostId,
                    OpponentName = hostName,
                    Winner = winnerName,
                    MatchDate = DateTime.UtcNow
                };
                _context.MatchHistory.Add(guestHistory);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalActiveMatchesAsync()
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            return await _context.MatchRequests
                .CountAsync(m => m.Status == "Matched" || m.Status == "Active");
        }

        public async Task<int> GetPlayersInMatchesAsync()
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var matches = await _context.MatchRequests
                .Where(m => m.Status == "Matched" || m.Status == "Active")
                .ToListAsync();

            return matches.Sum(m => m.GuestId.HasValue
                ? (m.GuestId == -1 ? 1 : 2)
                : 1);
        }

        public async Task<MatchRequest?> RematchAsync(string gameId, string user1Id, string user2Id)
        {
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(user1Id) || string.IsNullOrEmpty(user2Id))
            {
                return null;
            }

            // Verificar que user1Id y user2Id sean diferentes
            if (user1Id == user2Id)
            {
                Console.WriteLine("⚠️ Error: El solicitante y el oponente no pueden ser el mismo usuario.");
                return null;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var existingMatch = await context.MatchRequests
                .FirstOrDefaultAsync(m => m.GameId == gameId && m.Status == "Finished");
            if (existingMatch == null)
            {
                Console.WriteLine("⚠️ La partida no está terminada o no se encuentra en la base de datos.");
                return null;
            }

            var user1 = await context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == user1Id);
            var user2 = await context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == user2Id);
            if (user1 == null || user2 == null)
            {
                Console.WriteLine("⚠️ Uno o ambos jugadores no existen.");
                return null;
            }

            var newMatch = new MatchRequest
            {
                GameId = Guid.NewGuid().ToString(),
                HostId = user1.Id,
                GuestId = user2.Id,
                Status = "Pending",
                IsInvitedMatch = true,
                IsBotGame = false,
                CreatedAt = DateTime.UtcNow
            };

            context.MatchRequests.Add(newMatch);
            await context.SaveChangesAsync();

            Console.WriteLine($"Nueva revancha creada entre {user1Id} y {user2Id} con el GameId: {newMatch.GameId}");
            return newMatch;
        }

        public async Task<string> GetGameStatusAsync(string gameId)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            using DataContext _context = scope.ServiceProvider.GetRequiredService<DataContext>();
            var game = await _context.MatchRequests.FirstOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
            {
                return "Game not found";
            }

            return game.Status;
        }

    }
}