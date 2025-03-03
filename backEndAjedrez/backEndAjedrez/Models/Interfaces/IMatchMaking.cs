using System.Threading.Tasks;
using backEndAjedrez.Models.Database.Entities;

namespace backEndAjedrez.Models.Interfaces
{
    public interface IMatchMaking
    {
        Task<MatchRequest?> InviteFriendAsync(int hostId, string friendId);
        Task<MatchRequest> FindOrCreateMatchAsync(int userId);
        Task<MatchRequest> CreateBotMatchAsync(int hostId);
        Task<MatchRequest?> GetRoomByIdAsync(string gameId);
        Task<bool> RemoveRoomAsync(string gameId, int userId);
        Task<bool> RemovePlayerFromMatchAsync(string gameId, int userId);
        Task<bool> IsInvitedMatchAsync(string gameId);
        Task<bool> AcceptMatchInvitationAsync(string gameId, int userId);
        Task<bool> RejectMatchInvitationAsync(string gameId, int userId);
        Task<string?> GetGameIdByUserAsync(string userId);
        Task<string?> GetOpponentIdAsync(string userId, string gameId);
        Task<MatchRequest> GetMatchByGameIdAsync(string gameId);
        Task UpdateMatchStatusAsync(string gameId, string status);
        Task SendMatchUpdateAsync(string gameId, string message);
        Task SendMessageToUser(string userId, string message);
        Task SaveMatchHistoryAsync(string gameId, string winner);
    }
}
