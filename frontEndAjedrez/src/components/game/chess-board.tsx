"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import { useWebsocketContext } from "@/contexts/webContext-Context";

const initialPieces = {
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
  const [pieces, setPieces] = useState<{ [key: string]: string }>(initialPieces);
  const [validMoves, setValidMoves] = useState<string[]>([]);
  const { sendMessage, socket, gameId } = useWebsocketContext();

  // Función para determinar si una pieza es blanca (tuya)
  const isWhitePiece = (piece: string): boolean => {
    return piece.includes("/white/");
  };

  // Convertir coordenadas de ajedrez a "x,y"
  const chessToCartesian = (coordinate: string): string => {
    const col = columns.indexOf(coordinate[0]);
    const row = 8 - parseInt(coordinate[1]);
    return `${col},${row}`;
  };

  // Convertir coordenadas "x,y" de vuelta a notación de ajedrez
  const cartesianToChess = (x: number, y: number): string => {
    const col = columns[x];
    const row = 8 - y;
    return `${col}${row}`;
  };

  // Manejar el arrastre y pedir movimientos válidos
  const handleDragStart = (e: React.DragEvent, piece: string, coordinate: string) => {
    e.dataTransfer.setData("piece", piece);
    e.dataTransfer.setData("from", coordinate);

    const position = chessToCartesian(coordinate);
    if (gameId) {
      sendMessage("getValidMoves", gameId, position);
    }
  };

  // Escuchar mensajes del WebSocket
  useEffect(() => {
    if (!socket) return;

    const handleMessage = (event: MessageEvent) => {
      const message = JSON.parse(event.data);

      // Movimientos válidos del jugador
      if (message.success && message.validMoves) {
        const moves = message.validMoves.map((move: { x: number; y: number }) =>
          cartesianToChess(move.x, move.y)
        );
        setValidMoves(moves);
      }

      // Movimiento del rival (bot o jugador)
      if (message.success && message.move && message.status === "Move") {
        const { startX, startY, endX, endY } = message.move;
        const from = cartesianToChess(startX, startY); // Ej. "B8"
        const to = cartesianToChess(endX, endY);       // Ej. "A6"

        // Actualizar el estado de las piezas
        setPieces((prev) => {
          const updatedPieces = { ...prev };
          const piece = updatedPieces[from]; // Obtener la pieza del rival
          if (piece) {
            delete updatedPieces[from]; // Quitar de la posición inicial
            updatedPieces[to] = piece;  // Mover a la posición final
          }
          return updatedPieces;
        });
      }
    };

    socket.addEventListener("message", handleMessage);
    return () => {
      socket.removeEventListener("message", handleMessage);
    };
  }, [socket]);

  // Limpiar los movimientos válidos al terminar el arrastre
  const handleDragEnd = () => {
    setValidMoves([]);
  };

  // Manejar el soltado y enviar el movimiento del jugador solo si cambia la posición
  const handleDrop = (e: React.DragEvent, to: string) => {
    e.preventDefault();
    const piece = e.dataTransfer.getData("piece");
    const from = e.dataTransfer.getData("from");

    // Si la pieza se suelta en la misma casilla, no hacemos nada
    if (from === to || !validMoves.includes(to)) {
      setValidMoves([]);
      return;
    }

    const fromPosition = chessToCartesian(from); // Ej. "E2" -> "4,6"
    const toPosition = chessToCartesian(to);     // Ej. "E4" -> "4,4"
    const move = `${fromPosition},${toPosition}`; // Ej. "4,6,4,4"

    if (gameId) {
      sendMessage("makeMove", gameId, move);
    }

    setPieces((prev) => {
      const updatedPieces = { ...prev };
      delete updatedPieces[from];
      updatedPieces[to] = piece;
      return updatedPieces;
    });
    setValidMoves([]);
  };

  return (
    <div className="flex justify-center items-center h-screen">
      <div className="flex flex-col">
        <div className="flex">
          <div className="w-8 h-8"></div>
          {columns.map((col) => (
            <div key={col} className="w-16 h-16 flex justify-center items-center">
              <p className="text-center font-bold">{col}</p>
            </div>
          ))}
        </div>
        {rows.map((_, rowIndex) => (
          <div key={rowIndex} className="flex">
            <div className="w-16 h-16 flex justify-center items-center">
              <p className="text-center font-bold">{8 - rowIndex}</p>
            </div>
            {rows.map((_, colIndex) => {
              const isBlack = (rowIndex + colIndex) % 2 === 1;
              const coordinate = `${columns[colIndex]}${8 - rowIndex}`;
              const isValidMove = validMoves.includes(coordinate);
              const piece = pieces[coordinate];
              const canDrag = piece && isWhitePiece(piece); // Solo blancas son arrastrables
              return (
                <div
                  key={`${rowIndex}-${colIndex}`}
                  onDrop={(e) => handleDrop(e, coordinate)}
                  onDragOver={(e) => e.preventDefault()}
                  className={`w-16 h-16 ${
                    isValidMove ? "bg-yellow-500" : isBlack ? "bg-black" : "bg-white"
                  } flex justify-center items-center border`}
                >
                  {piece && (
                    <Image
                      src={piece}
                      width={50}
                      height={50}
                      draggable={canDrag} // Solo true para piezas blancas
                      onDragStart={canDrag ? (e) => handleDragStart(e, piece, coordinate) : undefined}
                      onDragEnd={canDrag ? handleDragEnd : undefined}
                      className={`cursor-${canDrag ? "pointer" : "default"}`}
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