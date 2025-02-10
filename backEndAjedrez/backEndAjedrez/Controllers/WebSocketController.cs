using backEndAjedrez.Services;
using backEndAjedrez.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace backEndAjedrez.Controllers;

[Route("api/")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketService _websocketService;
    private readonly WebSocketNetwork _websocketNetwork;
    private readonly Handler _handler;

    public WebSocketController(WebSocketService websocketService, WebSocketNetwork websocketNetwork, Handler handler)
    {
        _websocketService = websocketService;
        _websocketNetwork = websocketNetwork;
        _handler = handler;
       
    }

    [HttpGet]
    public async Task ConnectAsync()
    {
        // Si la petición es de tipo websocket la aceptamos
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Aceptamos la solicitud
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Manejamos la solicitud.
            await _websocketService.HandleAsync(webSocket);
        }
        // En caso contrario la rechazamos
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        // Cuando este método finalice, se cerrará automáticamente la conexión con el websocket
    }

    [HttpGet("handler")]
    public async Task ConnectHandler()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Aceptamos la solicitud
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Manejamos la conexión y pasamos el HttpContext
            await _handler.HandleAsync(HttpContext, webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpGet("advanced")]
    public async Task AdvancedConnectAsync()
    {
        // Si la petición es de tipo websocket la aceptamos
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Aceptamos la solicitud
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Manejamos la solicitud.
            await _websocketNetwork.HandleAsync(webSocket);
        }
        // En caso contrario la rechazamos
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        // Cuando este método finalice, se cerrará automáticamente la conexión con el websocket
    }
}

