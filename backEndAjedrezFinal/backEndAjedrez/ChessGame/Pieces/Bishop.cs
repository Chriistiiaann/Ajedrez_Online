using backEndAjedrez.ChessGame.Pieces;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace backEndAjedrez.Chess_Game.Pieces;
//Alfil
public class Bishop : Piece
{
    public override string Symbol => Color == "White" ? "B" : "b";

    public Bishop(string color) : base(color) { }

    public override bool IsValidMove(int startX, int startY, int endX, int endY, Board board)
    {
        if (Math.Abs(startX - endX) != Math.Abs(startY - endY))
            return false;

        int xDirection = (endX > startX) ? 1 : -1;
        int yDirection = (endY > startY) ? 1 : -1;
        int x = startX + xDirection;
        int y = startY + yDirection;

        while (x != endX && y != endY)
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

        foreach (int dx in directions)
        {
            foreach (int dy in directions)
            {
                int x = startX + dx;
                int y = startY + dy;

                while (x >= 0 && x < 8 && y >= 0 && y < 8)
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
                    y += dy;
                }
            }
        }

        return validMoves;
    }
}

