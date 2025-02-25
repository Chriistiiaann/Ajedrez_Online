using backEndAjedrez.ChessGame.Pieces;

namespace backEndAjedrez.Chess_Game.Pieces;

public class Knight : Piece
{
    public override string Symbol => Color == "White" ? "KN" : "kn";

    public Knight(string color) : base(color) { }

    public override bool IsValidMove(int startX, int startY, int endX, int endY, Board board)
    {
        bool tipo1 = (Math.Abs(startX - endX) == 2 && Math.Abs(startY - endY) == 1);
        bool tipo2 = (Math.Abs(startX - endX) == 1 && Math.Abs(startY - endY) == 2);

        if (tipo1 || tipo2)
        {
            Piece targetPiece = board.GetPiece(endX, endY);
            return targetPiece == null || targetPiece.Color != this.Color;
        }

        return false;
    }

    public override List<(int, int)> GetValidMoves(int startX, int startY, Board board)
    {
        List<(int, int)> validMoves = new List<(int, int)>();
        int[,] knightMoves = {
            { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 },
            { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 }
        };

        for (int i = 0; i < knightMoves.GetLength(0); i++)
        {
            int x = startX + knightMoves[i, 0];
            int y = startY + knightMoves[i, 1];
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                Piece targetPiece = board.GetPiece(x, y);
                if (targetPiece == null || targetPiece.Color != this.Color)
                {
                    validMoves.Add((x, y));
                }
            }
        }

        return validMoves;
    }
}