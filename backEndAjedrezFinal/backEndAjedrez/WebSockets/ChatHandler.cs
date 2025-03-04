using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Services;
using System.Net.WebSockets;
using System.Text.Json;
using System.Collections.Concurrent;

namespace backEndAjedrez.WebSockets;

public class ChatHandler
{
    private readonly MatchMakingService _matchMakingService;

    public ChatHandler(MatchMakingService matchMakingService)
    {
        _matchMakingService = matchMakingService;
    }

    public async Task HandleChatMessage(string userId, string gameId, string messageContent, ConcurrentDictionary<string, WebSocket> connections)
    {
        var match = await _matchMakingService.GetMatchByGameIdAsync(gameId);
        if (match == null || (match.Status != "Active" && match.Status != "Matched"))
        {
            await SendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "La partida no está activa." }), connections);
            return;
        }

        if (userId != match.HostId.ToString() && userId != match.GuestId.ToString())
        {
            await SendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No eres parte de esta partida." }), connections);
            return;
        }

        var senderInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));
        var senderAvatar = senderInfo.Avatar;
        var senderName = senderInfo.NickName;

        string receiverId = userId == match.HostId.ToString() ? match.GuestId.ToString() : match.HostId.ToString();

        var receiverMessage = new ChatDTO
        {
            gameChatMessage = true,
            GameId = gameId,
            SenderId = userId,
            SenderName = senderName,
            SenderAvatar = senderAvatar,
            Message = messageContent,
            IsSender = false
        };

        var senderMessage = new ChatDTO
        {
            gameChatMessage = true,
            GameId = gameId,
            SenderId = userId,
            SenderName = senderName,
            SenderAvatar = senderAvatar,
            Message = messageContent,
            IsSender = true
        };

        await SendMessageToUser(receiverId, JsonSerializer.Serialize(receiverMessage), connections);
        await SendMessageToUser(userId, JsonSerializer.Serialize(senderMessage), connections);
    }

    private async Task SendMessageToUser(string userId, string message, ConcurrentDictionary<string, WebSocket> connections)
    {
        if (connections.TryGetValue(userId, out var webSocket) && webSocket.State == WebSocketState.Open)
        {
            var response = System.Text.Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}