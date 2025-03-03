using backEndAjedrez.Chess_Game;
using System.Collections.Concurrent;

namespace backEndAjedrez.WebSockets;

public class GameBoardManager
{
    private readonly ConcurrentDictionary<string, Board> _activeBoards = new();

    public void InitializeBoard(string gameId)
    {
        _activeBoards.TryAdd(gameId, new Board());
    }

    public Board GetBoard(string gameId)
    {
        _activeBoards.TryGetValue(gameId, out var board);
        return board;
    }

    public bool ContainsBoard(string gameId)
    {
        return _activeBoards.ContainsKey(gameId);
    }

    public void RemoveBoard(string gameId)
    {
        _activeBoards.TryRemove(gameId, out _);
    }
}