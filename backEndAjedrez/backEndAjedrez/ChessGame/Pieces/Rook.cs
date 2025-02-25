using backEndAjedrez.ChessGame.Pieces;

namespace backEndAjedrez.Chess_Game.Pieces;

public class Rook : Piece
{
    public override string Symbol => Color == "White" ? "R" : "r";
    public bool HasMoved { get; private set; } = false;

    public Rook(string color) : base(color) { }

    public override bool IsValidMove(int startX, int startY, int endX, int endY, Board board)
    {
        // La torre solo se mueve en línea recta (horizontal o vertical)
        if (startX != endX && startY != endY)
            return false;

        int xDirection = (endX > startX) ? 1 : (endX < startX) ? -1 : 0;
        int yDirection = (endY > startY) ? 1 : (endY < startY) ? -1 : 0;

        int x = startX + xDirection;
        int y = startY + yDirection;

        // Verificar que no haya piezas en el camino
        while (x != endX || y != endY)
        {
            if (board.GetPiece(x, y) != null)
                return false;

            x += xDirection;
            y += yDirection;
        }

        Piece targetPiece = board.GetPiece(endX, endY);
        return targetPiece == null || targetPiece.Color != this.Color;
    }

    public override List<(int, int)> GetValidMoves(int startX, int startY, Board board)
    {
        List<(int, int)> validMoves = new List<(int, int)>();
        int[] directions = { -1, 1 };

        // Movimientos horizontales
        foreach (int dx in directions)
        {
            int x = startX + dx;
            int y = startY;

            while (x >= 0 && x < 8)
            {
                if (board.GetPiece(x, y) == null)
                {
                    validMoves.Add((x, y));
                }
                else
                {
                    if (board.GetPiece(x, y).Color != this.Color)
                        validMoves.Add((x, y));
                    break;
                }
                x += dx;
            }
        }

        // Movimientos verticales
        foreach (int dy in directions)
        {
            int x = startX;
            int y = startY + dy;

            while (y >= 0 && y < 8)
            {
                if (board.GetPiece(x, y) == null)
                {
                    validMoves.Add((x, y));
                }
                else
                {
                    if (board.GetPiece(x, y).Color != this.Color)
                        validMoves.Add((x, y));
                    break;
                }
                y += dy;
            }
        }

        return validMoves;
    }

    public void Move()
    {
        HasMoved = true;
    }
}


