using backEndAjedrez.Chess_Game.Pieces;
using backEndAjedrez.ChessGame.Pieces;
using System.Drawing;

namespace backEndAjedrez.Chess_Game;

public class Board
{
    public Piece[,] Grid { get; private set; }
    public bool IsWhiteTurn { get; private set; } = true;
    public Board()
    {
        Grid = new Piece[8, 8];
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        Grid = new Piece[8, 8]; // Asegurar que la matriz está vacía antes de llenarla

        // Torres
        Grid[0, 0] = new Rook("Black");
        Grid[7, 0] = new Rook("Black");
        Grid[0, 7] = new Rook("White");
        Grid[7, 7] = new Rook("White");

        // Caballos
        Grid[1, 0] = new Knight("Black");
        Grid[6, 0] = new Knight("Black");
        Grid[1, 7] = new Knight("White");
        Grid[6, 7] = new Knight("White");

        //// Alfiles
        Grid[2, 0] = new Bishop("Black");
        Grid[5, 0] = new Bishop("Black");
        Grid[2, 7] = new Bishop("White");
        Grid[5, 7] = new Bishop("White");

        //// Reinas
        Grid[3, 0] = new Queen("Black");
        Grid[3, 7] = new Queen("White");

        // Reyes
        Grid[4, 0] = new King("Black");
        Grid[4, 7] = new King("White");

        // Peones Negros (Fila 1)
        for (int i = 0; i < 8; i++)
        {
            Grid[i, 1] = new Pawn("Black");
        }

        // Peones Blancos (Fila 6)
        for (int i = 0; i < 8; i++)
        {
            Grid[i, 6] = new Pawn("White");
        }
    }

    public Piece GetPiece(int x, int y) => Grid[x, y];

    public void PrintBoard()
    {
        Console.WriteLine("   a  b  c  d  e  f  g  h");
        Console.WriteLine("  -------------------------");

        for (int y = 7; y >= 0; y--)
        {
            Console.Write((y + 1) + " | ");

            for (int x = 0; x < 8; x++)
            {
                if (Grid[x, y] == null)
                    Console.Write(".  ");
                else
                    Console.Write($"{Grid[x, y].Symbol,-2} ");
            }

            Console.WriteLine("| " + (y + 1));
        }

        Console.WriteLine("  -------------------------");
        Console.WriteLine("   a  b  c  d  e  f  g  h");
    }

    public void MovePiece(int startX, int startY, int endX, int endY)
    {
        Piece piece = Grid[startX, startY];
        if (piece == null)
        {
            Console.WriteLine("No hay una pieza en la posición de origen.");
            return;
        }

        if (piece.IsValidMove(startX, startY, endX, endY, this))
        {
            if (piece is King && Math.Abs(startX - endX) == 2)
            {
                int rookStartX = (endX > startX) ? 7 : 0;
                int rookEndX = (endX > startX) ? startX + 1 : startX - 1; // f o d
                Piece rook = Grid[rookStartX, startY];

                Grid[endX, endY] = piece;
                Grid[startX, startY] = null;
                ((King)piece).Move();

                Grid[rookEndX, startY] = rook;
                Grid[rookStartX, startY] = null;
                if (rook is Rook) ((Rook)rook).Move();

                Console.WriteLine($"Enroque realizado: Rey de ({startX},{startY}) a ({endX},{endY}), Torre de ({rookStartX},{startY}) a ({rookEndX},{startY})");
            }
            else
            {
                Grid[endX, endY] = piece;
                Grid[startX, startY] = null;
                if (piece is King) ((King)piece).Move();
                else if (piece is Rook) ((Rook)piece).Move();
                Console.WriteLine($"Pieza {piece.Symbol} movida de ({startX},{startY}) a ({endX},{endY})");
            }
            IsWhiteTurn = !IsWhiteTurn;
        }
        else
        {
            Console.WriteLine("Movimiento inválido.");
        }
    }

    public (int x, int y) EncontrarRey(string color)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece pieza = Grid[x, y];
                if (pieza != null && pieza is King && pieza.Color == color)
                {
                    return (x, y);
                }
            }
        }
        throw new Exception("Rey no encontrado en el tablero.");
    }

    public bool EstaEnJaque(string color)
    {
        (int reyX, int reyY) = EncontrarRey(color);
        string colorOponente = (color == "White") ? "Black" : "White";

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece pieza = Grid[x, y];
                if (pieza != null && pieza.Color == colorOponente)
                {
                    if (pieza is King)
                    {
                        int dX = Math.Abs(x - reyX);
                        int dY = Math.Abs(y - reyY);
                        if (dX <= 1 && dY <= 1 && (dX != 0 || dY != 0))
                        {
                            Console.WriteLine($"El rey {color} está en jaque por el rey {colorOponente} en ({x}, {y}).");
                            return true;
                        }
                    }
                    else
                    {
                        List<(int, int)> movimientos = pieza.GetValidMoves(x, y, this);
                        if (movimientos.Contains((reyX, reyY)))
                        {
                            Console.WriteLine($"El rey {color} está en jaque por {pieza.Symbol} en ({x}, {y}).");
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool EstaEnJaqueMate(string color)
    {
        if (!EstaEnJaque(color))
        {
            Console.WriteLine($"No hay jaque para {color}, por lo tanto no hay jaque mate.");
            return false;
        }

        Console.WriteLine($"Verificando jaque mate para {color}...");

        for (int startX = 0; startX < 8; startX++)
        {
            for (int startY = 0; startY < 8; startY++)
            {
                Piece pieza = Grid[startX, startY];
                if (pieza != null && pieza.Color == color)
                {
                    List<(int, int)> movimientos = pieza.GetValidMoves(startX, startY, this);
                    foreach (var (endX, endY) in movimientos)
                    {
                        Piece piezaDestino = Grid[endX, endY];
                        Grid[endX, endY] = pieza;
                        Grid[startX, startY] = null;

                        bool sigueEnJaque = EstaEnJaque(color);

                        Grid[startX, startY] = pieza;
                        Grid[endX, endY] = piezaDestino;

                        if (!sigueEnJaque)
                        {
                            Console.WriteLine($"Movimiento salvador encontrado: {pieza.Symbol} de ({startX},{startY}) a ({endX},{endY})");
                            return false;
                        }
                    }
                }
            }
        }

        Console.WriteLine($"¡Jaque mate confirmado para {color}!");
        return true;
    }
    public bool IsSquareAttacked(int x, int y, string color)
    {
        string colorOponente = (color == "White") ? "Black" : "White";

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece pieza = Grid[i, j];
                if (pieza != null && pieza.Color == colorOponente)
                {
                    if (pieza is King)
                    {
                        int dX = Math.Abs(i - x);
                        int dY = Math.Abs(j - y);
                        if (dX <= 1 && dY <= 1 && (dX != 0 || dY != 0))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        List<(int, int)> movimientos = pieza.GetValidMoves(i, j, this);
                        if (movimientos.Contains((x, y)))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public Board Clone()
    {
        Board clonedBoard = new Board();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Grid[x, y] != null)
                {
                    clonedBoard.Grid[x, y] = Grid[x, y].Clone();
                }
            }
        }

        return clonedBoard;
    }
}
