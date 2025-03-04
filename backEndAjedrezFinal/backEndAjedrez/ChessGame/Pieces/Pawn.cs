using backEndAjedrez.ChessGame.Pieces;
using System.Linq;

namespace backEndAjedrez.Chess_Game.Pieces;
//Peon
public class Pawn : Piece
{
    public override string Symbol => Color == "White" ? "P " : "p";

    public Pawn(string color) : base(color) { }
    public override bool IsValidMove(int startX, int startY, int endX, int endY, Board board)
    {
        int direction = (this.Color.Equals("White")) ? -1 : 1; // Blancas (-1), Negras (+1)

        if (startX == endX && endY == startY + direction)
        {
            if (board.GetPiece(endX, endY) == null)
                return true;
        }

        int startRow = (this.Color.Equals("White")) ? 6 : 1;
        if (startX == endX && startY == startRow && endY == startY + (2 * direction))
        {
            if (board.GetPiece(endX, endY) == null && board.GetPiece(endX, startY + direction) == null)
                return true;
        }

        if (Math.Abs(endX - startX) == 1 && endY == startY + direction)
        {
            Piece targetPiece = board.GetPiece(endX, endY);
            if (targetPiece != null && targetPiece.Color != this.Color)
                return true;
        }

        return false;
    }


    public override List<(int, int)> GetValidMoves(int startX, int startY, Board board)
    {
        List<(int, int)> validMoves = new List<(int, int)>();
        int directions = (this.Color.Equals("White")) ? -1 : 1;

        int fX = startX;
        int fY = startY + directions;

        if (fY >= 0 && fY < 8 && board.GetPiece(fX, fY) == null)
        {
            validMoves.Add((fX, fY));

            int startRow = (this.Color.Equals("White")) ? 6 : 1;
            int doubleMove = startY + (2 * directions);

            if (startY == startRow && board.GetPiece(fX, doubleMove)== null)
            {
                validMoves.Add((fX, doubleMove));
            }
        }

        int[] eatPiece = { -1, 1 };
        foreach (int dx in eatPiece)
        {
            int dX = startX + dx;
            int dY = startY + directions;

            if (dX >= 0 && dX <8 && dY >= 0 && dY < 8)
            {
                Piece targetPiece = board.GetPiece(dX, dY);
                if (targetPiece != null && targetPiece.Color != this.Color)
                {
                    validMoves.Add((dX, dY));
                }
            }
        }
        return validMoves;
    }
}
