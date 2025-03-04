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

    public async Task<(bool Success, bool IsCheckmate)> HandleMove(string userId, string gameId, string move, Func<string, string, Task> sendMessageToUser)
    {
        if (!_boardManager.ContainsBoard(gameId))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "Partida no encontrada." }));
            return (false, false);
        }

        var match = await _matchMakingService.GetMatchByGameIdAsync(gameId);
        if (match == null || match.Status != "Matched")
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "La partida no está activa." }));
            return (false, false);
        }

        var board = _boardManager.GetBoard(gameId);

        bool isUserWhite = match.HostId.ToString() == userId;
        bool isUserBlack = match.GuestId.ToString() == userId;

        if ((board.CurrentTurn == "White" && !isUserWhite) || (board.CurrentTurn == "Black" && !isUserBlack))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No es tu turno." }));
            return (false, false);
        }

        var coords = move.Split(',').Select(int.Parse).ToArray();
        var (startX, startY, endX, endY) = (coords[0], coords[1], coords[2], coords[3]);

        var piece = board.GetPiece(startX, startY);
        if (piece == null)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No hay pieza en la posición inicial." }));
            return (false, false);
        }

        if ((isUserWhite && piece.Color != "White") || (isUserBlack && piece.Color != "Black"))
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "No controlas esa pieza.", notYourTurn = true }));
            return (false, false);
        }

        var pieceAtStartBefore = board.GetPiece(startX, startY);
        var pieceAtEndBefore = board.GetPiece(endX, endY);

        board.MovePiece(startX, startY, endX, endY);

        var pieceAtStartAfter = board.GetPiece(startX, startY);
        var pieceAtEndAfter = board.GetPiece(endX, endY);

        if (pieceAtStartAfter == pieceAtStartBefore && pieceAtEndAfter == pieceAtEndBefore)
        {
            await sendMessageToUser(userId, JsonSerializer.Serialize(new { success = false, message = "Movimiento inválido.", invalidMove = true}));
            return (false, false);
        }

        board.CurrentTurn = board.CurrentTurn == "White" ? "Black" : "White";

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
            string winner = piece.Color;
            await _matchMakingService.SaveMatchHistoryAsync(gameId, winner);
            _boardManager.RemoveBoard(gameId);
            await _matchMakingService.UpdateMatchStatusAsync(gameId, "Finished");
            return (true, true);
        }

        if (match.IsBotGame && match.GuestId == -1 && piece.Color == "White")
        {
            await MakeBotMove(gameId, "Black", sendMessageToUser);
        }

        return (true, false);
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
        var match = await _matchMakingService.GetMatchByGameIdAsync(gameId);
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

        // Si el rey está en jaque, buscar un movimiento defensivo
        if (board.EstaEnJaque(botColor))
        {
            Console.WriteLine($"Bot ({botColor}) en jaque para GameId: {gameId}. Buscando movimiento defensivo...");

            var validDefensiveMoves = new List<(int startX, int startY, int endX, int endY)>();

            foreach (var (startX, startY, piece) in botPieces)
            {
                var moves = piece.GetValidMoves(startX, startY, board);
                foreach (var (endX, endY) in moves)
                {
                    var destinationPiece = board.GetPiece(endX, endY);
                    // Filtrar capturas de piezas aliadas antes de simular
                    if (destinationPiece != null && destinationPiece.Color == botColor)
                    {
                        Console.WriteLine($"Descartado movimiento inválido: ({startX},{startY}) a ({endX},{endY}) captura pieza aliada.");
                        continue;
                    }

                    // Simular el movimiento
                    Piece capturedPiece = destinationPiece;
                    board.MovePiece(startX, startY, endX, endY);

                    if (!board.EstaEnJaque(botColor))
                    {
                        validDefensiveMoves.Add((startX, startY, endX, endY));
                    }

                    // Restaurar el tablero
                    board.MovePiece(endX, endY, startX, startY);
                    // Nota: No podemos restaurar capturedPiece sin PlacePiece, pero filtramos antes para minimizar el impacto
                }
            }

            if (validDefensiveMoves.Count > 0)
            {
                // Elegir un movimiento válido y ejecutarlo solo una vez
                var Move = validDefensiveMoves[random.Next(validDefensiveMoves.Count)];
                var (startX, startY, endX, endY) = Move;

                board.MovePiece(startX, startY, endX, endY);

                var botData = new
                {
                    success = true,
                    gameId = gameId,
                    move = new { startX, startY, endX, endY },
                    status = "Move",
                    message = "El bot ha escapado del jaque."
                };
                string jsonBotMove = JsonSerializer.Serialize(botData);
                await sendMessageToUser(match.HostId.ToString(), jsonBotMove);
                Console.WriteLine($"Bot movió de ({startX},{startY}) a ({endX},{endY}) para salir del jaque.");
                return;
            }

            // Jaque mate si no hay movimientos válidos
            Console.WriteLine($"Bot no puede salir del jaque en GameId: {gameId}. Jaque mate.");
            var checkmateData = new
            {
                success = true,
                gameId = gameId,
                status = "Checkmate",
                message = "¡Jaque mate! El jugador gana."
            };
            string jsonCheckmate = JsonSerializer.Serialize(checkmateData);
            await sendMessageToUser(match.HostId.ToString(), jsonCheckmate);
            _boardManager.RemoveBoard(gameId);
            await _matchMakingService.UpdateMatchStatusAsync(gameId, "Finished");
            return;
        }

        // Movimiento aleatorio si no está en jaque
        var allValidMoves = new List<(int startX, int startY, int endX, int endY)>();
        foreach (var (x, y, piece) in botPieces)
        {
            var moves = piece.GetValidMoves(x, y, board);
            foreach (var (endX, endY) in moves)
            {
                var destinationPiece = board.GetPiece(endX, endY);
                if (destinationPiece == null || destinationPiece.Color != botColor)
                {
                    allValidMoves.Add((x, y, endX, endY));
                }
                else
                {
                    Console.WriteLine($"Descartado movimiento inválido: ({x},{y}) a ({endX},{endY}) captura pieza aliada.");
                }
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
        await sendMessageToUser(match.HostId.ToString(), botJsonResponse);
        Console.WriteLine($"Bot movió aleatoriamente de ({botStartX},{botStartY}) a ({botEndX},{botEndY})");

        if (botGameStatus == "Checkmate")
        {
            _boardManager.RemoveBoard(gameId);
            await _matchMakingService.UpdateMatchStatusAsync(gameId, "Finished");
        }
    }
}