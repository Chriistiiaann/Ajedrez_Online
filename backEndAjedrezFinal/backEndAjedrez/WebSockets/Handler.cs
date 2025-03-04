using backEndAjedrez.Models.Dtos;
using backEndAjedrez.Services;
using Microsoft.Extensions.Hosting;
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
    private readonly GameMoveHandler _gameMoveHandler;
    private readonly GameBoardManager _boardManager;
    private readonly ChatHandler _chatHandler;


    public Handler(FriendService friendService, StatusService statusService, MatchMakingService matchMakingService, GameMoveHandler gameMoveHandler, GameBoardManager boardManager, ChatHandler chatHandler)
    {
        _friendService = friendService;
        _statusService = statusService;
        _matchMakingService = matchMakingService;
        _gameMoveHandler = gameMoveHandler;
        _boardManager = boardManager;
        _chatHandler = chatHandler;
    }
    private void InitializeBoard(string gameId)
    {
        if (!_boardManager.ContainsBoard(gameId))
        {
            _boardManager.InitializeBoard(gameId);
            Console.WriteLine($"Tablero inicializado para gameId: {gameId}");
        }
    }
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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

        await SendStatsToAll();

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
                                var invitedInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));
                                var messageDto = new 
                                {
                                    message = success
                                        ? $"{invitedInfo.NickName} te ha enviado una solicitud de amistad."
                                        : "Ya existe una solicitud pendiente o no puedes enviarla a ti mismo.",
                                    friendRequestSent = success 
                                        ? true
                                        : false
                                };

                                var jsonMessage = JsonSerializer.Serialize(messageDto);
                                await SendMessageToUser(success ? userId : userId, jsonMessage);

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
                                            : "Error al aceptar la solicitud.",
                                    friendRequestAccepted = true
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
                                           : "Error al rechazar la solicitud.",
                                     friendRequestAccepted = false
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
                                var user1 = await _friendService.GetUserByIdAsync(int.Parse(userId));
                                if (user1.Role == "Banned")
                                {
                                    await SendMessageToUser(userId,
                                        System.Text.Json.JsonSerializer.Serialize(new { message = "❌ No puedes invitar a este usuario, tu cuenta está baneada." })
                                    ); break;
                                }
                                var friend = await _friendService.GetUserByIdAsync(int.Parse(friendId.ToString()));
                                if (friend.Role == "Banned")
                                {
                                    await SendMessageToUser(userId,
                                        System.Text.Json.JsonSerializer.Serialize(new { message = "❌ Este usuario está baneado y no puedes invitarlo a jugar." })
                                    );
                                    break;
                                }
                                var match = await _matchMakingService.InviteFriendAsync(int.Parse(userId), friendId);

                                if (match != null)
                                {
                                    var userInfoInvitation = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));

                                    var invitationData = new
                                    {
                                        senderId = userInfoInvitation.Id,
                                        senderNickname = userInfoInvitation.NickName,
                                        senderAvatar = userInfoInvitation.Avatar,
                                        gameId = match.GameId,
                                        playerColor = "w",
                                        message = $"🎮 {userInfoInvitation.NickName} te ha invitado a jugar. Partida ID: {match.GameId}",
                                        gameInvitation = true
                                    };
                                    string jsonInvitation = System.Text.Json.JsonSerializer.Serialize(invitationData);

                                    await SendMessageToUser(friendId.ToString(), jsonInvitation);
                                    var confirmationData = new
                                    {
                                        success = true,
                                        gameId = match.GameId,
                                        invitedFriendId = friendId,
                                        playerColor = "w",
                                        message = "✅ Invitación enviada con éxito.",
                                        gameInvitation = true
                                    };
                                    string jsonConfirmation = System.Text.Json.JsonSerializer.Serialize(confirmationData);
                                    await SendMessageToUser(userId, jsonConfirmation);
                                    await SendStatsToAll();
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
                                var gameId7 = request["gameId"];
                                var user2 = await _friendService.GetUserByIdAsync(int.Parse(userId));
                                if (user2.Role == "Banned")
                                {
                                    await SendMessageToUser(userId,
                                        System.Text.Json.JsonSerializer.Serialize(new { message = "❌ No puedes aceptar partidas, tu cuenta está baneada." })
                                    ); break;
                                }
                                bool accepted = await _matchMakingService.AcceptMatchInvitationAsync(gameId7, int.Parse(userId));
                                if (accepted)
                                {
                                    InitializeBoard(gameId7);
                                    var hostId = _matchMakingService.GetHostId(gameId7);
                                    var invitedInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(hostId));

                                    var responseInfo = new
                                    {
                                        senderId = invitedInfo.Id,
                                        senderNickname = invitedInfo.NickName,
                                        senderAvatar = invitedInfo.Avatar,
                                        gameId = gameId7,
                                        playerColor = "b",
                                        message = $"🎮 Has aceptado la invitación. ¡Partida {gameId7} iniciada!",
                                        acceptGameInvitation = true
                                    };

                                    var userInfoAceptInvitation = await _matchMakingService.GetUserInfoAsync(int.Parse(userId));
                                    var responseHost = new
                                    {
                                        senderId = userInfoAceptInvitation.Id,
                                        senderNickname = userInfoAceptInvitation.NickName,
                                        senderAvatar = userInfoAceptInvitation.Avatar,
                                        gameId = gameId7,
                                        playerColor = "b",
                                        message = $"🎮 Han aceptado la invitación. ¡Partida {gameId7} iniciada!",
                                        acceptGameInvitation = true
                                    };

                                    string jsonInvitation = System.Text.Json.JsonSerializer.Serialize(responseHost);
                                    var response = JsonSerializer.Serialize(responseInfo);
                                    var responseHosts = JsonSerializer.Serialize(responseHost);

                                    await SendMessageToUser(userId, response);
                                    await SendMessageToUser(hostId, responseHosts);
                                    await SendStatsToAll();
                                }
                                
                            }
                            break;

                        case "rejectMatchInvitation":
                            if (request.ContainsKey("gameId"))
                            {
                                var gameId6 = request["gameId"];
                                var user3 = await _friendService.GetUserByIdAsync(int.Parse(userId));
                                if (user3.Role == "Banned")
                                {
                                    await SendMessageToUser(userId,
                                        System.Text.Json.JsonSerializer.Serialize(new { message = "❌ No puedes rechazar partidas, tu cuenta está baneada." })
                                    ); break;
                                }
                                bool rejected = await _matchMakingService.RejectMatchInvitationAsync(gameId6, int.Parse(userId));

                                var responseInfo = new
                                {
                                    success = rejected,
                                    gameId = gameId6,
                                    message = rejected
                                            ? $"❌ Has rechazado la invitación a la partida."
                                            : "⚠ No se pudo rechazar la invitación.",
                                    acceptGameInvitation = false
                                };
                                var responseHost = new
                                {
                                    success = rejected,
                                    gameId = gameId6,
                                    message = rejected
                                            ? $"❌ Han rechazado la invitación a la partida."
                                            : "⚠ No se pudo rechazar la invitación.",
                                    acceptGameInvitation = false
                                };

                                var hostId = _matchMakingService.GetHostId(gameId6);
                                var responseHosts = JsonSerializer.Serialize(responseHost);

                                var response = JsonSerializer.Serialize(responseInfo);
                                await SendMessageToUser(userId, response);
                                await SendMessageToUser(hostId, responseHosts);

                            }
                            break;

                        case "findRandomMatch":
                            var user = await _friendService.GetUserByIdAsync(int.Parse(userId));
                            if (user.Role == "Banned")
                            {
                                await SendMessageToUser(userId,
                                    System.Text.Json.JsonSerializer.Serialize(new { message = "❌ No puedes buscar partidas, tu cuenta está baneada." })
                                ); break;
                            }
                            var matchRandom = await _matchMakingService.FindOrCreateMatchAsync(int.Parse(userId));

                            if (matchRandom.GuestId != null)
                            {
                                InitializeBoard(matchRandom.GameId);

                                var hostInfo = await _matchMakingService.GetUserInfoAsync(matchRandom.HostId);
                                var guestInfo = await _matchMakingService.GetUserInfoAsync(matchRandom.GuestId.Value);

                                var hostData = new
                                {
                                    success = true,
                                    opponentId = matchRandom.GuestId,
                                    opponentNickName = guestInfo.NickName, 
                                    opponentAvatar = guestInfo.Avatar,
                                    gameId = matchRandom.GameId,
                                    playerColor = "w",
                                    message = $"✅ Emparejado con {guestInfo.NickName}. Partida: {matchRandom.GameId}"
                                };

                                var guestData = new
                                {
                                    success = true,
                                    opponentId = matchRandom.HostId,
                                    opponentNickName = hostInfo.NickName, 
                                    opponentAvatar = hostInfo.Avatar,
                                    gameId = matchRandom.GameId,
                                    playerColor = "b",
                                    message = $"✅ Emparejado con {hostInfo.NickName}. Partida: {matchRandom.GameId}"
                                };

                                string jsonHost = JsonSerializer.Serialize(hostData);
                                string jsonGuest = JsonSerializer.Serialize(guestData);

                                await SendMessageToUser(matchRandom.HostId.ToString(), jsonHost);
                                await SendMessageToUser(matchRandom.GuestId.ToString(), jsonGuest);
                                await SendStatsToAll();
                            }
                            else
                            {
                                var searchingData = new
                                {
                                    success = false,
                                    searchingOpponent = true,
                                    gameId = (int?)null,
                                    message = "🔍 Buscando oponente..."
                                };

                                string jsonSearching = JsonSerializer.Serialize(searchingData);
                                await SendMessageToUser(userId, jsonSearching);
                            }
                            break;

                        case "playWithBot":
                            var user4 = await _friendService.GetUserByIdAsync(int.Parse(userId));
                            if (user4.Role == "Banned")
                            {
                                await SendMessageToUser(userId,
                                    System.Text.Json.JsonSerializer.Serialize(new { message = "❌ No puedes jugar partidas, tu cuenta está baneada." })
                                ); break;
                            }
                            var matchBot = await _matchMakingService.CreateBotMatchAsync(int.Parse(userId));
                            InitializeBoard(matchBot.GameId);
                            var responseData = new
                            {
                                success = true,
                                gameId = matchBot.GameId,
                                opponent = "bot",
                                playerColor = "w",
                                message = $"🤖 Partida contra bot creada. Partida: {matchBot.GameId}"
                            };

                            string jsonResponse = JsonSerializer.Serialize(responseData);
                            await SendMessageToUser(userId, jsonResponse);
                            await SendStatsToAll();
                            break;

                        case "rematch":
                            string userIdRemtach = request["userId"];
                            string? gameId = await _matchMakingService.GetGameIdByUserAsync(userIdRemtach);

                            if (gameId == null)
                            {
                                await SendMessageToUser(userIdRemtach, "❌ No estás en ninguna partida, no puedes solicitar una revancha.");
                                break;
                            }

                            string? gameStatus = await _matchMakingService.GetGameStatusAsync(gameId);
                            if (gameStatus != "Finished")
                            {
                                await SendMessageToUser(userIdRemtach, "❌ La partida aún no ha terminado, no puedes solicitar una revancha.");
                                break;
                            }

                            string? opponentId = await _matchMakingService.GetOpponentIdAsync(userIdRemtach, gameId);
                            if (opponentId == null)
                            {
                                await SendMessageToUser(userIdRemtach, "❌ No se pudo encontrar al oponente para la revancha.");
                                break;
                            }

                            if (userIdRemtach == opponentId)
                            {
                                await SendMessageToUser(userIdRemtach, "❌ Error: El oponente y el solicitante son el mismo usuario.");
                                break;
                            }

                            var rematchRequest = await _matchMakingService.RematchAsync(gameId, userIdRemtach, opponentId);
                            if (rematchRequest == null)
                            {
                                await SendMessageToUser(userIdRemtach, "❌ Error al crear la revancha.");
                                break;
                            }

                            var userInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(userIdRemtach));
                            var opponentInfo = await _matchMakingService.GetUserInfoAsync(int.Parse(opponentId));

                            var rematchRequestData = new
                            {
                                success = true,
                                gameId = rematchRequest.GameId,
                                userNickname = userInfo.NickName,
                                opponentNickname = opponentInfo.NickName,
                                message = $"🎮 {userInfo.NickName} te ha retado a una revancha. ¿Aceptas?"
                            };
                            await SendMessageToUser(opponentId, System.Text.Json.JsonSerializer.Serialize(rematchRequestData));

                            var rematchAcceptedData = new
                            {
                                success = true,
                                gameId = rematchRequest.GameId,
                                message = "✅ Solicitud de revancha enviada."
                            };
                            await SendMessageToUser(userIdRemtach, System.Text.Json.JsonSerializer.Serialize(rematchAcceptedData));
                            break;



                        case "makeMove":
                            if (request.ContainsKey("gameId") && request.ContainsKey("move"))
                            {
                                var gameId2 = request["gameId"];
                                var move = request["move"];
                                var (success, isCheckmate) = await _gameMoveHandler.HandleMove(userId, gameId2, move, SendMessageToUser);
                                if (success && isCheckmate)
                                {
                                    await SendStatsToAll();
                                }
                            }
                            break;

                        case "getValidMoves":
                            if (request.ContainsKey("gameId") && request.ContainsKey("position"))
                            {
                                var gameId3 = request["gameId"];
                                var position = request["position"];
                                await _gameMoveHandler.GetValidMoves(userId, gameId3, position, SendMessageToUser);
                            }
                            break;

                        case "sendChatMessage":
                            if (request.ContainsKey("gameId") && request.ContainsKey("message"))
                            {
                                var gameId4 = request["gameId"];
                                var messageContent = request["message"];
                                await _chatHandler.HandleChatMessage(userId, gameId4, messageContent, _connections);
                            }
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
            await SendStatsToAll();
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
                        await SendMessageToUser(opponentId, jsonResponse);
                    }
                }
                _semaphore.Release();
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

    public async Task SendStatsToAll()
    {
        int totalUsersConnected = _connections.Count;
        int totalMatches = await _matchMakingService.GetTotalActiveMatchesAsync();
        int playersInMatches = await _matchMakingService.GetPlayersInMatchesAsync();

        var message = JsonSerializer.Serialize(new 
            { 
                totalUsersConnected,
                totalMatches,  
                playersInMatches,
            });

        foreach (var webSocket in _connections.Values)
        {

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}