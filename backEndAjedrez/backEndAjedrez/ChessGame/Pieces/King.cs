using backEndAjedrez.ChessGame.Pieces;

namespace backEndAjedrez.Chess_Game.Pieces;
//Rey
public class King : Piece
{
    public override string Symbol => Color == "White" ? "K" : "k";
    public bool HasMoved { get; private set; } = false;


    public King(string color) : base(color) { }
    public override bool IsValidMove(int startX, int startY, int endX, int endY, Board board)
    {
        int dX = Math.Abs(startX - endX);
        int dY = Math.Abs(startY - endY);

        if (dX > 1 || dY > 1)
            return false;

        if (endX < 0 || endX >= 8 || endY < 0 || endY >= 8)
            return false;

        Piece targetPiece = board.GetPiece(endX, endY);
        return targetPiece == null || targetPiece.Color != this.Color;
    }
    public override List<(int, int)> GetValidMoves(int startX, int startY, Board board)
    {
        List<(int, int)> validMoves = new List<(int, int)>();
        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dy in directions)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int x = startX + dx;
                int y = startY + dy;

                if (x >= 0 && x < 8 && y >= 0 && y < 8)
                {
                    Piece targetPiece = board.GetPiece(x, y);

                    if (targetPiece == null || targetPiece.Color != this.Color)
                    {
                        validMoves.Add((x, y));
                    }
                }
            }
        }

        return validMoves;
    }

}
