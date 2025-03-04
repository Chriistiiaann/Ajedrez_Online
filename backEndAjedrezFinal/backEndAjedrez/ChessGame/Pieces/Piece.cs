using backEndAjedrez.Chess_Game;

namespace backEndAjedrez.ChessGame.Pieces;
public abstract class Piece
{
    public string Color { get; set; }
    public int X { get; set; }  // Agregar coordenada X
    public int Y { get; set; }  // Agregar coordenada Y
    public abstract string Symbol { get; }

    protected Piece(string color)
    {
        Color = color;
    }
    public virtual Piece Clone()
    {
        return (Piece)this.MemberwiseClone();
    }


    public abstract bool IsValidMove(int startX, int startY, int endX, int endY, Board board);
    public abstract List<(int, int)> GetValidMoves(int startX, int startY, Board board);
}
