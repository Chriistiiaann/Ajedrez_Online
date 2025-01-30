"use client";

import { useState } from "react";
import Image from "next/image";

// Posiciones iniciales de las piezas con rutas absolutas
const initialPieces: { [key: string]: string } = {
  A8: "/chess-pieces/black/black_rook.svg",
  B8: "/chess-pieces/black/black_knight.svg",
  C8: "/chess-pieces/black/black_bishop.svg",
  D8: "/chess-pieces/black/black_queen.svg",
  E8: "/chess-pieces/black/black_king.svg",
  F8: "/chess-pieces/black/black_bishop.svg",
  G8: "/chess-pieces/black/black_knight.svg",
  H8: "/chess-pieces/black/black_rook.svg",
  A7: "/chess-pieces/black/black_pawn.svg",
  B7: "/chess-pieces/black/black_pawn.svg",
  C7: "/chess-pieces/black/black_pawn.svg",
  D7: "/chess-pieces/black/black_pawn.svg",
  E7: "/chess-pieces/black/black_pawn.svg",
  F7: "/chess-pieces/black/black_pawn.svg",
  G7: "/chess-pieces/black/black_pawn.svg",
  H7: "/chess-pieces/black/black_pawn.svg",
  A2: "/chess-pieces/white/white_pawn.svg",
  B2: "/chess-pieces/white/white_pawn.svg",
  C2: "/chess-pieces/white/white_pawn.svg",
  D2: "/chess-pieces/white/white_pawn.svg",
  E2: "/chess-pieces/white/white_pawn.svg",
  F2: "/chess-pieces/white/white_pawn.svg",
  G2: "/chess-pieces/white/white_pawn.svg",
  H2: "/chess-pieces/white/white_pawn.svg",
  A1: "/chess-pieces/white/white_rook.svg",
  B1: "/chess-pieces/white/white_knight.svg",
  C1: "/chess-pieces/white/white_bishop.svg",
  D1: "/chess-pieces/white/white_queen.svg",
  E1: "/chess-pieces/white/white_king.svg",
  F1: "/chess-pieces/white/white_bishop.svg",
  G1: "/chess-pieces/white/white_knight.svg",
  H1: "/chess-pieces/white/white_rook.svg",
};

const Chessboard: React.FC = () => {
  const rows = Array(8).fill(null);
  const columns = ["A", "B", "C", "D", "E", "F", "G", "H"];

  // Estado para las posiciones de las piezas
  const [pieces, setPieces] = useState<{ [key: string]: string }>(initialPieces);

  // Función para manejar el arrastre
  const handleDragStart = (e: React.DragEvent, piece: string, coordinate: string) => {
    e.dataTransfer.setData("piece", piece);
    e.dataTransfer.setData("from", coordinate);
  };

  // Función para manejar el soltar
  const handleDrop = (e: React.DragEvent, to: string) => {
    e.preventDefault();
    const piece = e.dataTransfer.getData("piece");
    const from = e.dataTransfer.getData("from");

    setPieces((prev) => {
      const updatedPieces = { ...prev };
      delete updatedPieces[from]; // Quitar la pieza de la casilla original
      updatedPieces[to] = piece; // Colocar la pieza en la nueva casilla
      return updatedPieces;
    });
  };

  return (
    <div className="flex justify-center items-center h-screen">
      <div className="flex flex-col">
        {/* Letras (A-H) encima del tablero */}
        <div className="flex">
          <div className="w-8 h-8"></div> {/* Espacio vacío para alinear */}
          {columns.map((col) => (
            <div key={col} className="w-16 h-16 flex justify-center items-center">
              <p className="text-center font-bold">{col}</p>
            </div>
          ))}
        </div>

        {/* Tablero con números (8-1) a la izquierda */}
        {rows.map((_, rowIndex) => (
          <div key={rowIndex} className="flex">
            {/* Números (8-1) a la izquierda */}
            <div className="w-16 h-16 flex justify-center items-center">
              <p className="text-center font-bold">{8 - rowIndex}</p>
            </div>

            {/* Casillas del tablero */}
            {rows.map((_, colIndex) => {
              const isBlack = (rowIndex + colIndex) % 2 === 1;
              const coordinate = `${columns[colIndex]}${8 - rowIndex}`;
              return (
                <div
                  key={`${rowIndex}-${colIndex}`}
                  onDrop={(e) => handleDrop(e, coordinate)}
                  onDragOver={(e) => e.preventDefault()}
                  className={`w-16 h-16 ${
                    isBlack ? "bg-black" : "bg-white"
                  } flex justify-center items-center border`}
                >
                  {/* Renderiza pieza si existe en la posición */}
                  {pieces[coordinate] && (
                    <Image
                      src={pieces[coordinate]}
                      width={50}
                      height={50}
                      draggable
                      onDragStart={(e) => handleDragStart(e, pieces[coordinate], coordinate)}
                      className="cursor-pointer"
                      alt="chess piece"
                    />
                  )}
                </div>
              );
            })}
          </div>
        ))}
      </div>
    </div>
  );
};

export default Chessboard;
