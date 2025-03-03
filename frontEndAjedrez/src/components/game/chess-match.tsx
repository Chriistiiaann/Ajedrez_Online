"use client";

import { useState, useCallback, useEffect } from "react";
import { Chess } from "chess.js";
import { Send } from "lucide-react";
import ChessBoard from "./chess-board";
import { useWebsocketContext } from "@/contexts/webContext-Context";

export default function ChessMatch({ gameId }: { gameId: string }) {
    const [game, setGame] = useState(new Chess());
    const [newMessage, setNewMessage] = useState("");
    const [moveHistory, setMoveHistory] = useState<string[]>([]); // Estado para el historial de movimientos
    const [hasClearedChat, setHasClearedChat] = useState(false);
    const { sendMessage, chatMessages, clearChatMessages } = useWebsocketContext();

    // Limpiar el chat solo una vez al cambiar de partida
    useEffect(() => {
        if (!hasClearedChat) {
            clearChatMessages();
            setHasClearedChat(true);
        }
    }, [gameId, clearChatMessages, hasClearedChat]);

    // Resetear hasClearedChat cuando gameId cambie
    useEffect(() => {
        setHasClearedChat(false);
    }, [gameId]);

    // Actualizar el historial de movimientos cuando cambie el estado del juego
    useEffect(() => {
        setMoveHistory(game.history());
    }, [game]);

    // Manejar mensajes del WebSocket para movimientos del oponente
    useEffect(() => {
        const socket = new WebSocket(`wss://localhost:7218/api/handler?userId=${localStorage.getItem("userId")}`);

        if (!socket) return;

        const handleMessage = (event: MessageEvent) => {
            const message = JSON.parse(event.data);

            if (message.success && message.move && message.status === "Move" && message.gameId === gameId) {
                const { startX, startY, endX, endY } = message.move;
                const from = String.fromCharCode(97 + startX) + (8 - startY); // Convertir a notación como "e2"
                const to = String.fromCharCode(97 + endX) + (8 - endY);       // Convertir a notación como "e4"

                const move = {
                    from,
                    to,
                    promotion: "q", // Asumimos promoción a reina por simplicidad
                };

                try {
                    game.move(move);
                    setGame(new Chess(game.fen())); // Actualizar el estado del juego
                    console.log(`Movimiento del oponente: ${from} -> ${to}`);
                } catch (error) {
                    console.error("Error al procesar movimiento del oponente:", error);
                }
            }
        };

        socket.addEventListener("message", handleMessage);
        return () => {
            socket.removeEventListener("message", handleMessage);
        };
    }, [gameId]);

    const sendMessageHandler = useCallback(
        (message: string) => {
            if (message.trim() === "") return;
            sendMessage("sendChatMessage", gameId, message.trim());
            setNewMessage("");
        },
        [sendMessage, gameId]
    );

    // Manejar movimientos del jugador desde ChessBoard
    const onDrop = useCallback(
        (sourceSquare: string, targetSquare: string) => {
            const move = {
                from: sourceSquare,
                to: targetSquare,
                promotion: "q", // Promoción a reina por simplicidad
            };

            try {
                const result = game.move(move);
                if (result === null) return false;

                setGame(new Chess(game.fen())); // Actualizar el estado del juego
                sendMessage("makeMove", gameId, `${sourceSquare},${targetSquare}`); // Enviar al WebSocket
                console.log(`Movimiento enviado: ${sourceSquare} -> ${targetSquare}`);
                return true;
            } catch (error) {
                console.error("Movimiento inválido:", error);
                return false;
            }
        },
        [game, sendMessage, gameId]
    );

    return (
        <div className="container mx-auto px-4 py-4 max-w-7xl">
            <div className="bg-foreground shadow-md rounded-lg p-4 mb-6">
                <div className="flex justify-between items-center">
                    <div className="flex items-center">
                        <div className="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center text-gray-700 font-bold">
                            P1
                        </div>
                        <div className="ml-3">
                            <p className="font-semibold">Player 1</p>
                        </div>
                    </div>

                    <div className="text-center">
                        <div className="text-2xl font-bold">VS</div>
                        <div className="text-sm text-gray-500">Game #{gameId}</div>
                    </div>

                    <div className="flex items-center">
                        <div className="mr-3 text-right">
                            <p className="font-semibold">Player 2</p>
                        </div>
                        <div className="w-10 h-10 bg-gray-800 rounded-full flex items-center justify-center text-white font-bold">
                            P2
                        </div>
                    </div>
                </div>
            </div>

            <div className="flex flex-col lg:flex-row gap-6 mb-6">
                <div className="flex-1">
                    <div className="bg-foreground shadow-md rounded-lg p-4">
                        <div style={{ margin: "0 auto" }} className="flex justify-center">
                            <ChessBoard onDrop={onDrop} /> {/* Pasamos onDrop al ChessBoard */}
                        </div>
                    </div>
                </div>

                <div className="lg:w-1/3 flex flex-col bg-foreground shadow-md rounded-lg overflow-hidden h-[500px] lg:h-auto">
                    <div className="p-3 bg-accent border-b">
                        <h2 className="font-semibold">Game Chat</h2>
                    </div>

                    <div className="flex-1 overflow-y-auto p-4 space-y-3">
                        {chatMessages.map((msg, index) => (
                            <div key={index} className={`flex ${msg.IsSender ? "justify-end" : "justify-start"}`}>
                                <div className={`max-w-[80%] rounded-lg px-3 py-2 ${msg.IsSender ? "bg-blue-400" : "bg-gray-100"}`}>
                                    <p className="text-xs text-gray-500">{msg.SenderName}</p>
                                    <p>{msg.Message}</p>
                                </div>
                            </div>
                        ))}
                    </div>

                    <div className="p-3 border-t">
                        <div className="flex">
                            <input
                                type="text"
                                value={newMessage}
                                onChange={(e) => setNewMessage(e.target.value)}
                                className="flex-1 border rounded-l-lg px-3 py-2 focus:outline-none"
                                placeholder="Type a message..."
                                onKeyDown={(e) => {
                                    if (e.key === "Enter") {
                                        sendMessageHandler(newMessage);
                                        setNewMessage("");
                                    }
                                }}
                            />
                            <button
                                onClick={() => {
                                    sendMessageHandler(newMessage);
                                    setNewMessage("");
                                }}
                                className="bg-blue-500 text-white px-3 py-2 rounded-r-lg"
                            >
                                <Send size={18} />
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-foreground shadow-md rounded-lg p-4">
                    <h2 className="text-xl font-semibold mb-2">Historial de movimientos</h2>
                    <div className="h-48 overflow-y-auto">
                        {moveHistory.length > 0 ? (
                            <div className="grid grid-cols-2 gap-2">
                                {moveHistory.map((move, index) => (
                                    <div key={index} className="text-white">
                                        {index % 2 === 0 && (
                                            <span className="mr-2">{Math.floor(index / 2) + 1}.</span>
                                        )}
                                        {move}
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <p className="text-sm text-gray-400">No hay movimientos aún</p>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}