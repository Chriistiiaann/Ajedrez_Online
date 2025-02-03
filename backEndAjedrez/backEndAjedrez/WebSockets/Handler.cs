using backEndAjedrez.Services;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;


namespace backEndAjedrez.WebSockets;
public class Handler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly FriendService _friendService;  // Corregido el nombre aquí
    private readonly StatusService _statusService;

    // Constructor para inyectar FriendService
    public Handler(FriendService friendService, StatusService statusService)
    {
        _friendService = friendService;
        _statusService = statusService;
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
                    if (action == "sendFriendRequest" && request.ContainsKey("toUserId"))
                    {
                        var toUserId = request["toUserId"];
                        bool success = await _friendService.SendFriendRequest(userId, toUserId);

                        if (success)
                        {
                            await SendMessageToUser(toUserId, $"{userId} te ha enviado una solicitud de amistad.");
                        }
                        else
                        {
                            await SendMessageToUser(userId, "Ya existe una solicitud pendiente o no puedes enviarla a ti mismo.");
                        }
                    }
                    else if (action == "acceptFriendRequest" && request.ContainsKey("requestId"))
                    {
                        int requestId = int.Parse(request["requestId"]);
                        bool success = await _friendService.AcceptFriendRequest(requestId);

                        if (success)
                        {
                            await SendMessageToUser(userId, "Solicitud de amistad aceptada.");
                        }
                        else
                        {
                            await SendMessageToUser(userId, "Error al aceptar la solicitud.");
                        }
                    }
                    else if (action == "rejectFriendRequest" && request.ContainsKey("requestId"))
                    {
                        int requestId = int.Parse(request["requestId"]);
                        bool success = await _friendService.RejectFriendRequest(requestId);

                        if (success)
                        {
                            await SendMessageToUser(userId, "Solicitud de amistad rechazada.");
                        }
                        else
                        {
                            await SendMessageToUser(userId, "Error al rechazar la solicitud.");
                        }
                    }
                    else if (action == "getPendingRequests")
                    {
                        var pendingRequests = await _friendService.GetPendingRequests(userId);
                        await SendMessageToUser(userId, JsonSerializer.Serialize(pendingRequests));
                    }

                    if (action == "changeStatus" && request.ContainsKey("userId") && request.ContainsKey("status"))
                    {
                        var idUser = int.Parse(request["userId"]);
                        var newStatus = request["status"];

                        var changeStatus = await _statusService.ChangeStatusAsync(idUser, newStatus);
                    }
                }
            }

            // Cuando el usuario se conecta, enviamos automáticamente sus solicitudes pendientes
            var pendingRequestsOnConnect = await _friendService.GetPendingRequests(userId);
            if (pendingRequestsOnConnect.Any())
            {
                await SendMessageToUser(userId, JsonSerializer.Serialize(pendingRequestsOnConnect));
            }
        }
        finally
        {
            var idUser = int.Parse(userId);
            _connections.TryRemove(userId, out _);
            await SendTotalUsersConnectedToAll();

            await _statusService.ChangeStatusAsync(idUser, "Disconnected");
            Console.WriteLine($"❌ Usuario {userId} desconectado");
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
        // Contar el número total de usuarios conectados
        int totalUsersConnected = _connections.Count;

        // Crear el mensaje en formato JSON
        var message = JsonSerializer.Serialize(new { totalUsersConnected });

        // Enviar el mensaje a todos los clientes conectados
        foreach (var webSocket in _connections.Values)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}


