using backEndAjedrez.Chess_Game;
using backEndAjedrez.ChessGame.Pieces;
using backEndAjedrez.Models.Database.Entities;
using backEndAjedrez.Services;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace backEndAjedrez.WebSockets;

public class GameMoveHandler
{
    private readonly GameBoardManager _boardManager;
    private readonly MatchMakingService _matchMakingService;

    public GameMoveHandler(GameBoardManager boardManager, MatchMakingService matchMakingService)
    {
        _boardManager = boardManager;
        _matchMakingService = matchMakingService;
    }

    public async Task HandleMove(string userId, string gameId, string move, Func<string, string, Task> sendMessageToUser)
    {
        if (!_boardManager.ContainsBoard(gameId))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "Partida no encontrada." }));
            return;
        }

        var match = await _matchMakingService.GetMatchByGameIdAsync(gameId);
        if (match == null || match.Status != "Matched")
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "La partida no está activa." }));
            return;
        }

        var coords = move.Split(',').Select(int.Parse).ToArray();
        var (startX, startY, endX, endY) = (coords[0], coords[1], coords[2], coords[3]);

        var board = _boardManager.GetBoard(gameId);
        var piece = board.GetPiece(startX, startY);

        if (piece == null)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No hay pieza en la posición inicial." }));
            return;
        }

        bool isWhiteTurn = (match.HostId.ToString() == userId && piece.Color == "White") ||
                          (match.GuestId.ToString() == userId && piece.Color == "Black");

        if (!isWhiteTurn)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No es tu turno o no controlas esa pieza." }));
            return;
        }

        board.MovePiece(startX, startY, endX, endY);

        string opponentColor = piece.Color == "White" ? "Black" : "White";
        string gameStatus = board.EstaEnJaqueMate(opponentColor) ? "Checkmate" :
                            board.EstaEnJaque(opponentColor) ? "Check" : "Move";

        var moveData = new
        {
            success = true,
            gameId = gameId,
            move = new { startX, startY, endX, endY },
            status = gameStatus,
            message = gameStatus == "Checkmate" ? "¡Jaque mate! Partida terminada." :
                      gameStatus == "Check" ? "¡Jaque!" : "Movimiento realizado."
        };

        string jsonResponse = JsonSerializer.Serialize(moveData);

        await sendMessageToUser(match.HostId.ToString(), jsonResponse);
        if (match.GuestId.HasValue)
        {
            await sendMessageToUser(match.GuestId.Value.ToString(), jsonResponse);
        }

        if (gameStatus == "Checkmate")
        {
            _boardManager.RemoveBoard(gameId);
            await _matchMakingService.UpdateMatchStatusAsync(gameId, "Finished");
        }

        if (match.IsBotGame == true && match.GuestId == -1)
        {
            await MakeBotMove(gameId, opponentColor, sendMessageToUser);
        }
    }

    public async Task GetValidMoves(string userId, string gameId, string position, Func<string, string, Task> sendMessageToUser)
    {
        if (!_boardManager.ContainsBoard(gameId))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "Partida no encontrada." }));
            return;
        }

        var match = await _matchMakingService.GetMatchByGameIdAsync(gameId);
        if (match == null || (match.Status != "Active" && match.Status != "Matched"))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "La partida no está activa." }));
            return;
        }

        var coords = position.Split(',').Select(int.Parse).ToArray();
        if (coords.Length != 2)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "Posición inválida." }));
            return;
        }
        var (startX, startY) = (coords[0], coords[1]);

        var board = _boardManager.GetBoard(gameId);
        var piece = board.GetPiece(startX, startY);

        if (piece == null)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No hay pieza en esa posición." }));
            return;
        }

        bool isWhiteTurn = (match.HostId.ToString() == userId && piece.Color == "White") ||
                         (match.GuestId.ToString() == userId && piece.Color == "Black");

        if (!isWhiteTurn)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No es tu turno o no controlas esa pieza." }));
            return;
        }

        var validMoves = piece.GetValidMoves(startX, startY, board);

        var serializedMoves = validMoves.Select(m => new { x = m.Item1, y = m.Item2 }).ToList();

        var response = new
        {
            success = true,
            gameId = gameId,
            position = new { startX, startY },
            validMoves = serializedMoves
        };

        await sendMessageToUser(userId, JsonSerializer.Serialize(response));
    }

    private async Task MakeBotMove(string gameId, string botColor, Func<string, string, Task> sendMessageToUser)
    {
        var board = _boardManager.GetBoard(gameId);
        var random = new Random();

        var botPieces = new List<(int x, int y, Piece piece)>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var piece = board.GetPiece(x, y);
                if (piece != null && piece.Color == botColor)
                {
                    botPieces.Add((x, y, piece));
                }
            }
        }

        var allValidMoves = new List<(int startX, int startY, int endX, int endY)>();
        foreach (var (x, y, piece) in botPieces)
        {
            var validMoves = piece.GetValidMoves(x, y, board);
            foreach (var (endX, endY) in validMoves)
            {
                allValidMoves.Add((x, y, endX, endY));
            }
        }

        if (allValidMoves.Count == 0)
        {
            return;
        }

        var chosenMove = allValidMoves[random.Next(allValidMoves.Count)];
        var (botStartX, botStartY, botEndX, botEndY) = chosenMove;

        board.MovePiece(botStartX, botStartY, botEndX, botEndY);

        string playerColor = botColor == "White" ? "Black" : "White";
        string botGameStatus = board.EstaEnJaqueMate(playerColor) ? "Checkmate" :
                              board.EstaEnJaque(playerColor) ? "Check" : "Move";

        var botMoveData = new
        {
            success = true,
            gameId = gameId,
            move = new { startX = botStartX, startY = botStartY, endX = botEndX, endY = botEndY },
            status = botGameStatus,
            message = botGameStatus == "Checkmate" ? "¡Jaque mate! El bot gana." :
                      botGameStatus == "Check" ? "¡Jaque del bot!" : "El bot ha movido."
        };

        string botJsonResponse = JsonSerializer.Serialize(botMoveData);
        await sendMessageToUser("-1", botJsonResponse);
        await sendMessageToUser(_matchMakingService.GetMatchByGameIdAsync(gameId).Result.HostId.ToString(), botJsonResponse);

        if (botGameStatus == "Checkmate")
        {
            _boardManager.RemoveBoard(gameId);
            await _matchMakingService.UpdateMatchStatusAsync(gameId, "Finished");
        }
    }
}