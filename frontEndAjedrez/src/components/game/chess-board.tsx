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

interface ChessBoardProps {
    onMove?: (from: string, to: string, isOpponentMove?: boolean) => void;
}

const ChessBoard: React.FC<ChessBoardProps> = ({ onMove }) => {
    const rows = Array(8).fill(null);
    const columns = ["A", "B", "C", "D", "E", "F", "G", "H"];
    const [pieces, setPieces] = useState<{ [key: string]: string }>(initialPieces);
    const [validMoves, setValidMoves] = useState<string[]>([]);
    const { sendMessage, socket, gameId, gameStatus, playerColor, matchMakingState } = useWebsocketContext();

    const isPlayerPiece = (piece: string): boolean => {
        const effectiveColor = playerColor || (matchMakingState === "botMatch" ? "w" : null);
        if (!effectiveColor) return false;
        return effectiveColor === "w" ? piece.includes("/white/") : piece.includes("/black/");
    };

    const chessToCartesian = (coordinate: string): string => {
        const col = columns.indexOf(coordinate[0]);
        const row = 8 - parseInt(coordinate[1]);
        return `${col},${row}`;
    };

    const cartesianToChess = (x: number, y: number): string => {
        const col = columns[x];
        const row = 8 - y;
        return `${col}${row}`;
    };

    const handleDragStart = (e: React.DragEvent, piece: string, coordinate: string) => {
        e.dataTransfer.setData("piece", piece);
        e.dataTransfer.setData("from", coordinate);

        const position = chessToCartesian(coordinate);
        if (gameId) {
            sendMessage("getValidMoves", gameId, position);
        }
    };

    useEffect(() => {
        if (!socket) return;

        const handleMessage = (event: MessageEvent) => {
            const message = JSON.parse(event.data);

            if (message.success && message.validMoves) {
                const moves = message.validMoves.map((move: { x: number; y: number }) =>
                    cartesianToChess(move.x, move.y)
                );
                setValidMoves(moves);
            }

            if (message.success && message.move && message.status === "Move") {
                const { startX, startY, endX, endY } = message.move;
                const from = cartesianToChess(startX, startY);
                const to = cartesianToChess(endX, endY);

                setPieces((prev) => {
                    const updatedPieces = { ...prev };
                    const piece = updatedPieces[from];
                    if (piece) {
                        delete updatedPieces[from];
                        updatedPieces[to] = piece;
                        if (onMove) onMove(from, to, true); // Notificar movimiento del oponente
                    }
                    return updatedPieces;
                });
            }
        };

        socket.addEventListener("message", handleMessage);
        return () => {
            socket.removeEventListener("message", handleMessage);
        };
    }, [socket, onMove]);

    const handleDragEnd = () => {
        setValidMoves([]);
    };

    const handleDrop = (e: React.DragEvent, to: string) => {
        e.preventDefault();
        const piece = e.dataTransfer.getData("piece");
        const from = e.dataTransfer.getData("from");

        if (from === to || !validMoves.includes(to)) {
            setValidMoves([]);
            return;
        }

        const fromPosition = chessToCartesian(from);
        const toPosition = chessToCartesian(to);
        const move = `${fromPosition},${toPosition}`;

        if (gameId) {
            sendMessage("makeMove", gameId, move);
        }

        setPieces((prev) => {
            const updatedPieces = { ...prev };
            delete updatedPieces[from];
            updatedPieces[to] = piece;
            if (onMove) onMove(from, to); // Notificar movimiento del jugador
            return updatedPieces;
        });
        setValidMoves([]);
    };

    const effectiveColor = playerColor || (matchMakingState === "botMatch" ? "w" : null);
    const displayRows = effectiveColor === "b" ? [...rows].reverse() : rows;
    const displayColumns = effectiveColor === "b" ? [...columns].reverse() : columns;

    useEffect(() => {
        if (gameStatus) {
            console.log("Estado del juego:", gameStatus.status, gameStatus.message);
        }
    }, [gameStatus]);

    return (
        <div className="flex justify-center items-center h-screen">
            <div className="flex flex-col">
                <div className="flex">
                    <div className="w-8 h-8"></div>
                    {displayColumns.map((col) => (
                        <div key={col} className="w-16 h-16 flex justify-center items-center">
                            <p className="text-center font-bold">{col}</p>
                        </div>
                    ))}
                </div>
                {displayRows.map((_, rowIndex) => (
                    <div key={rowIndex} className="flex">
                        <div className="w-16 h-16 flex justify-center items-center">
                            <p className="text-center font-bold">
                                {effectiveColor === "b" ? rowIndex + 1 : 8 - rowIndex}
                            </p>
                        </div>
                        {displayRows.map((_, colIndex) => {
                            const isBlack = (rowIndex + colIndex) % 2 === 1;
                            const coordinate = `${displayColumns[colIndex]}${
                                effectiveColor === "b" ? rowIndex + 1 : 8 - rowIndex
                            }`;
                            const isValidMove = validMoves.includes(coordinate);
                            const piece = pieces[coordinate];
                            const canDrag = piece && isPlayerPiece(piece);
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
                                            draggable={canDrag}
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
                {gameStatus && (
                    <div className="text-center mt-4">
                        <p className="text-xl font-bold">
                            {gameStatus.status === "Check" ? "¡Jaque!" : "¡Jaque Mate!"}
                        </p>
                        <p>{gameStatus.message}</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ChessBoard;