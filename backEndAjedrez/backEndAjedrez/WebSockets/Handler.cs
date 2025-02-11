using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Services;
using Microsoft.AspNetCore.Http.HttpResults;
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
                                var responseInfo = new
                                {
                                    requestId = requestId,
                                    message = success
                                            ? "Solicitud de amistad aceptada."
                                            : "Error al aceptar la solicitud."
                                };

                                var response = JsonSerializer.Serialize(responseInfo);
                                await SendMessageToUser(userId, response);
                            }
                            break;

                        case "rejectFriendRequest":
                            if (request.ContainsKey("requestId"))
                            {
                                int requestId = int.Parse(request["requestId"]);
                                bool success = await _friendService.RejectFriendRequest(requestId);
                                var responseInfo = new
                                {
                                    requestId = requestId,
                                    message = success
                                           ? "Solicitud de amistad rechazada."
                                           : "Error al rechazar la solicitud."
                                };

                                var response = JsonSerializer.Serialize(responseInfo);
                                await SendMessageToUser(userId, response);
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
                                    var userInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));

                                    var invitationData = new
                                    {
                                        senderId = userInfo.Id,
                                        senderNickname = userInfo.NickName,
                                        senderAvatar = userInfo.Avatar,
                                        gameId = match.GameId,
                                        message = $"🎮 {userInfo.NickName} te ha invitado a jugar. Partida ID: {match.GameId}"
                                    };
                                    string jsonInvitation = System.Text.Json.JsonSerializer.Serialize(invitationData);

                                    await SendMessageToUser(friendId.ToString(), jsonInvitation);
                                    var confirmationData = new
                                    {
                                        success = true,
                                        gameId = match.GameId,
                                        invitedFriendId = friendId,
                                        message = "✅ Invitación enviada con éxito."
                                    };
                                    string jsonConfirmation = System.Text.Json.JsonSerializer.Serialize(confirmationData);
                                    await SendMessageToUser(userId, jsonConfirmation);
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

                                var responseInfo = new
                                {
                                    success = accepted,
                                    gameId = gameId,
                                    message = accepted
                                            ? $"🎮 Has aceptado la invitación. ¡Partida {gameId} iniciada!"
                                            : "❌ No se pudo aceptar la invitación."
                                };

                                var response = JsonSerializer.Serialize(responseInfo);
                                await SendMessageToUser(userId, response);
                            }
                            break;

                        case "rejectMatchInvitation":
                            if (request.ContainsKey("gameId"))
                            {
                                var gameId = request["gameId"];
                                bool rejected = await _matchMakingService.RejectMatchInvitationAsync(gameId, int.Parse(userId));

                                var responseInfo = new
                                {
                                    success = rejected,
                                    gameId = gameId,
                                    message = rejected
                                            ? $"❌ Has rechazado la invitación a la partida."
                                            : "⚠ No se pudo rechazar la invitación."
                                };

                                var response = JsonSerializer.Serialize(responseInfo);
                                await SendMessageToUser(userId, response);
                            }
                            break;

                        case "findRandomMatch":
                            var matchRandom = await _matchMakingService.FindOrCreateMatchAsync(int.Parse(userId));

                            if (matchRandom.GuestId != null)
                            {
                                var userInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));

                                var hostData = new
                                {
                                    success = true,
                                    opponentId = matchRandom.GuestId,
                                    opponentNickName = userInfo.NickName,
                                    opponentAvatar = userInfo.Avatar,
                                    gameId = matchRandom.GameId,
                                    message = $"✅ Emparejado con {userInfo.NickName}. Partida: {matchRandom.GameId}"
                                };

                                var guestData = new
                                {
                                    success = true,
                                    opponentId = matchRandom.HostId,
                                    opponentNickName = userInfo.NickName,
                                    opponentAvatar = userInfo.Avatar,
                                    gameId = matchRandom.GameId,
                                    message = $"✅ Emparejado con {userInfo.NickName}. Partida: {matchRandom.GameId}"
                                };

                                string jsonHost = JsonSerializer.Serialize(hostData);
                                string jsonGuest = JsonSerializer.Serialize(guestData);

                                await SendMessageToUser(matchRandom.HostId.ToString(), jsonHost);
                                await SendMessageToUser(matchRandom.GuestId.ToString(), jsonGuest);
                            }
                            else
                            {
                                var searchingData = new
                                {
                                    success = false,
                                    gameId = (int?)null,
                                    message = "🔍 Buscando oponente..."
                                };

                                string jsonSearching = JsonSerializer.Serialize(searchingData);
                                await SendMessageToUser(userId, jsonSearching);
                            }
                            break;

                        case "playWithBot":
                            var matchBot = await _matchMakingService.CreateBotMatchAsync(int.Parse(userId));

                            var responseData = new
                            {
                                success = true,
                                gameId = matchBot.GameId,
                                opponent = "bot",
                                message = $"🤖 Partida contra bot creada. Partida: {matchBot.GameId}"
                            };

                            string jsonResponse = JsonSerializer.Serialize(responseData);
                            await SendMessageToUser(userId, jsonResponse);
                            break;

                        default:
                            var errorResponse = new
                            {
                                success = false,
                                message = "❌ Acción no reconocida."
                            };

                            string jsonError = JsonSerializer.Serialize(errorResponse);
                            await SendMessageToUser(userId, jsonError);
                            break;

                    }
                }
            }

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
                            var userInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));
                            var responseData = new
                            {
                                success = true,
                                gameId = gameId,
                                userNickname = userInfo.NickName,
                                message = $"⚠️ Tu oponente {userInfo.NickName} se ha desconectado de la partida {gameId}."
                            };

                            string jsonResponse = JsonSerializer.Serialize(responseData);
                            await SendMessageToUser(opponentId, jsonResponse);
                        }
                    }
                }
                else
                {
                    string? opponentId = await _matchMakingService.GetOpponentIdAsync(userId, gameId);
                    if (opponentId != null)
                    {
                        var userInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));
                        var responseData = new
                        {
                            success = true,
                            gameId = gameId,
                            userNickname = userInfo.NickName,
                            message = $"⚠️ Tu amigo {userInfo.NickName} se ha desconectado de la partida {gameId}, pero puede volver."
                        };

                        string jsonResponse = JsonSerializer.Serialize(responseData);
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