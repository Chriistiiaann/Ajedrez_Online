using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Services;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace backEndAjedrez.WebSockets;
public class Handler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly FriendService _friendService;  
    private readonly StatusService _statusService;
    private readonly MatchMakingService _matchMakingService;

    
    public Handler(FriendService friendService, StatusService statusService, MatchMakingService matchMakingService)
    {
        _friendService = friendService;
        _statusService = statusService;
        _matchMakingService = matchMakingService;
    }

    public async Task HandleAsync(HttpContext context, WebSocket webSocket)
    {
        var userId = context.Request.Query["userId"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Falta el userId en la URL");
            return;
        }

        await _statusService.ChangeStatusAsync(int.Parse(userId), "Connected");
        _connections[userId] = webSocket;
        Console.WriteLine($"🔗 Usuario {userId} conectado");

        await SendTotalUsersConnectedToAll();

        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var request = JsonSerializer.Deserialize<Dictionary<string, string>>(message);

                if (request.TryGetValue("action", out string action))
                {
                    switch (action)
                    {
                        case "sendFriendRequest":
                            if (request.ContainsKey("toUserId"))
                            {
                                var toUserId = request["toUserId"];
                                bool success = await _friendService.SendFriendRequest(userId, toUserId);
                                var messageDto = new MessageDTO
                                {
                                    Message = success
                                        ? $"{userId} te ha enviado una solicitud de amistad."
                                        : "Ya existe una solicitud pendiente o no puedes enviarla a ti mismo."
                                };

                                var jsonMessage = JsonSerializer.Serialize(messageDto);

                                await SendMessageToUser(success ? toUserId : userId, jsonMessage);
                            }
                            break;

                        case "acceptFriendRequest":
                            if (request.ContainsKey("requestId"))
                            {
                                int requestId = int.Parse(request["requestId"]);
                                bool success = await _friendService.AcceptFriendRequest(requestId);

                                await SendMessageToUser(userId, success
                                    ? "Solicitud de amistad aceptada."
                                    : "Error al aceptar la solicitud.");
                            }
                            break;

                        case "rejectFriendRequest":
                            if (request.ContainsKey("requestId"))
                            {
                                int requestId = int.Parse(request["requestId"]);
                                bool success = await _friendService.RejectFriendRequest(requestId);

                                await SendMessageToUser(userId, success
                                    ? "Solicitud de amistad rechazada."
                                    : "Error al rechazar la solicitud.");
                            }
                            break;

                        case "changeStatus":
                            if (request.ContainsKey("userId") && request.ContainsKey("status"))
                            {
                                var idUser = int.Parse(request["userId"]);
                                var newStatus = request["status"];
                                await _statusService.ChangeStatusAsync(idUser, newStatus);
                            }
                            break;

                        case "inviteFriendToGame":
                            if (request.ContainsKey("friendId"))
                            {
                                var friendId = request["friendId"];

                                var match = await _matchMakingService.InviteFriendAsync(int.Parse(userId), friendId);

                                if (match != null)
                                {
                                    await SendMessageToUser(friendId.ToString(), $"🎮 Invitación a jugar de {userId}. Partida: {match.GameId}");
                                    await SendMessageToUser(userId, "✅ Invitación enviada.");
                                }
                                else
                                {
                                    await SendMessageToUser(userId, "❌ No puedes invitar a este usuario.");
                                }
                            }
                            break;

                        case "acceptMatchInvitation":
                            if (request.ContainsKey("gameId"))
                            {
                                var gameId = request["gameId"];
                                bool accepted = await _matchMakingService.AcceptMatchInvitationAsync(gameId, int.Parse(userId));

                                if (accepted)
                                {
                                    await SendMessageToUser(userId, "🎮 Has aceptado la invitación. ¡Partida iniciada!");
                                }
                                else
                                {
                                    await SendMessageToUser(userId, "❌ No se pudo aceptar la invitación.");
                                }
                            }
                            break;

                        case "rejectMatchInvitation":
                            if (request.ContainsKey("gameId"))
                            {
                                var gameId = request["gameId"];
                                bool rejected = await _matchMakingService.RejectMatchInvitationAsync(gameId, int.Parse(userId));

                                if (rejected)
                                {
                                    await SendMessageToUser(userId, "❌ Has rechazado la invitación a la partida.");
                                }
                                else
                                {
                                    await SendMessageToUser(userId, "⚠ No se pudo rechazar la invitación.");
                                }
                            }
                            break;

                        case "findRandomMatch":
                            var matchRandom = await _matchMakingService.FindOrCreateMatchAsync(int.Parse(userId));

                            if (matchRandom.GuestId != null)
                            {
                                await SendMessageToUser(matchRandom.HostId.ToString(), $"✅ Emparejado con {matchRandom.GuestId}. Partida: {matchRandom.GameId}");
                                await SendMessageToUser(matchRandom.GuestId.ToString(), $"✅ Emparejado con {matchRandom.HostId}. Partida: {matchRandom.GameId}");
                            }
                            else
                            {
                                await SendMessageToUser(userId, "🔍 Buscando oponente...");
                            }
                            break;

                        case "playWithBot":
                            var matchBot = await _matchMakingService.CreateBotMatchAsync(int.Parse(userId));
                            await SendMessageToUser(userId, $"🤖 Partida contra bot creada. Partida: {matchBot.GameId}");
                            break;

                        default:
                            await SendMessageToUser(userId, "❌ Acción no reconocida.");
                            break;
                    }
                }
            }

            // Enviar las solicitudes pendientes cuando el usuario se conecta
            var pendingRequestsOnConnect = await _friendService.GetPendingRequests(userId);
            if (pendingRequestsOnConnect.Any())
            {
                await SendMessageToUser(userId, JsonSerializer.Serialize(pendingRequestsOnConnect));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en WebSocket: {ex.Message}");
        }
        finally
        {
            _connections.TryRemove(userId, out _);
            await SendTotalUsersConnectedToAll();
            await _statusService.ChangeStatusAsync(int.Parse(userId), "Disconnected");
            Console.WriteLine($"❌ Usuario {userId} desconectado");

            string? gameId = await _matchMakingService.GetGameIdByUserAsync(userId);

            if (gameId != null)
            {
                int userIdInt = int.Parse(userId);

                bool isInvitedMatch = await _matchMakingService.IsInvitedMatchAsync(gameId);

                if (!isInvitedMatch)
                {
              
                    bool removed = await _matchMakingService.RemovePlayerFromMatchAsync(gameId, userIdInt);

                    if (removed)
                    {
                        Console.WriteLine($"🎮 Usuario {userId} eliminado de la partida {gameId}.");

                        string? opponentId = await _matchMakingService.GetOpponentIdAsync(userId, gameId);
                        if (opponentId != null)
                        {
                            await SendMessageToUser(opponentId, $"⚠️ Tu oponente {userId} se ha desconectado de la partida {gameId}.");
                        }
                    }
                }
                else
                {
                    string? opponentId = await _matchMakingService.GetOpponentIdAsync(userId, gameId);
                    if (opponentId != null)
                    {
                        await SendMessageToUser(opponentId, $"⚠️ Tu amigo {userId} se ha desconectado de la partida {gameId}, pero puede volver.");
                    }
                }
            }
        }
    }

    public async Task SendMessageToUser(string userId, string message)
    {
        if (_connections.TryGetValue(userId, out var webSocket) && webSocket.State == WebSocketState.Open)
        {
            var response = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    private async Task SendTotalUsersConnectedToAll()
    {
        int totalUsersConnected = _connections.Count;

        var message = JsonSerializer.Serialize(new { totalUsersConnected });

        foreach (var webSocket in _connections.Values)
        {

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}