"use client";

import { useState, useCallback, useEffect } from "react";
import { Send } from "lucide-react";
import ChessBoard from "./chess-board";
import { useWebsocketContext } from "@/contexts/webContext-Context";
import Image from "next/image";

export default function ChessMatch({ gameId }: { gameId: string }) {
    const [newMessage, setNewMessage] = useState("");
    const [hasClearedChat, setHasClearedChat] = useState(false);
    const { sendMessage, chatMessages, clearChatMessages, userData, opponentData } = useWebsocketContext();

    // Usar datos del contexto con valores por defecto si no están disponibles
    const userName = userData?.name || "Player 1";
    const userImage = userData?.image || "/placeholder.svg";
    const opponentName = opponentData?.name || "Player 2";
    // Usar "/profilePics/botAvatar.png" si el nombre del rival es "CPU"
    const opponentImage = opponentName === "CPU" ? "/profilePics/botAvatar.png" : (opponentData?.image || "/placeholder.svg");

    // Limpiar el chat solo una vez al cambiar de partida
    useEffect(() => {
        if (!hasClearedChat) {
            clearChatMessages();
            setHasClearedChat(true);
            console.log("Chat cleared for game:", gameId);
        }
    }, [gameId, clearChatMessages, hasClearedChat]);

    // Resetear hasClearedChat cuando gameId cambie
    useEffect(() => {
        setHasClearedChat(false);
    }, [gameId]);

    const sendMessageHandler = useCallback(
        (message: string) => {
            if (message.trim() === "") return;
            sendMessage("sendChatMessage", gameId, message.trim());
            setNewMessage("");
        },
        [sendMessage, gameId]
    );

    return (
        <div className="container mx-auto px-4 py-4 max-w-7xl">
            <div className="bg-foreground shadow-md rounded-lg p-4 mb-6">
                <div className="flex justify-between items-center">
                    <div className="flex items-center">
                        <div className="w-10 h-10 bg-gray-200 rounded-full flex items-center justify-center text-gray-700 font-bold overflow-hidden">
                            <Image
                                src={`https://localhost:7218/${userImage}` || "/placeholder.svg"}
                                alt={userName}
                                width={40}
                                height={40}
                                className="object-cover"
                            />
                        </div>
                        <div className="ml-3">
                            <p className="font-semibold">{userName}</p>
                        </div>
                    </div>

                    <div className="text-center">
                        <div className="text-2xl font-bold">VS</div>
                        <div className="text-sm text-gray-500">Game #{gameId}</div>
                    </div>

                    <div className="flex items-center">
                        <div className="mr-3 text-right">
                            <p className="font-semibold">{opponentName}</p>
                        </div>
                        <div className="w-10 h-10 bg-gray-800 rounded-full flex items-center justify-center text-white font-bold overflow-hidden">
                            <Image
                                src={opponentName === "CPU" ? "/profilePics/botAvatar.png" : `https://localhost:7218/${opponentImage}` || "/placeholder.svg"}
                                alt={opponentName}
                                width={40}
                                height={40}
                                className="object-cover"
                            />
                        </div>
                    </div>
                </div>
            </div>

            <div className="flex flex-col lg:flex-row gap-6 mb-6">
                <div className="flex-1">
                    <div className="bg-foreground shadow-md rounded-lg p-4">
                        <div style={{ margin: "0 auto" }} className="flex justify-center">
                            <ChessBoard />
                        </div>
                    </div>
                </div>

                <div className="lg:w-1/3 flex flex-col bg-foreground shadow-md rounded-lg overflow-hidden h-[500px] lg:h-auto">
                    <div className="p-3 bg-accent border-b">
                        <h2 className="font-semibold">Chat de juego</h2>
                    </div>

                    <div className="flex-1 overflow-y-auto p-4 space-y-3">
                        {chatMessages.map((msg, index) => (
                            <div key={index} className={`flex ${msg.IsSender ? "justify-end" : "justify-start"}`}>
                                <div className={`max-w-[80%] text-black rounded-lg px-3 py-2 ${msg.IsSender ? "bg-blue-400" : "bg-gray-100"}`}>
                                    <p className="text-xs text-gray-800">{msg.SenderName}</p>
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
                                className="flex-1 border rounded-l-lg px-3 py-2 focus:outline-none text-black"
                                placeholder="Escribe tu mensaje..."
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
                {/* <div className="bg-foreground shadow-md rounded-lg p-4">
                    <h2 className="text-xl font-semibold mb-2">historial de movimientos</h2>
                    <div className="h-48 overflow-y-auto">
                        
                    </div>
                </div> */}

                {/* <div className="bg-foreground shadow-md rounded-lg p-4">
                    <h2 className="text-xl font-semibold mb-2">Game Controls</h2>
                    <div className="grid grid-cols-2 gap-3">
                        <Button onClick={resetGame} className="flex items-center justify-center gap-2">
                            <Plus size={18} />
                            New Game
                        </Button>
                        <Button onClick={undoMove} className="flex items-center justify-center gap-2">
                            <RotateCcw size={18} />
                            Undo Move
                        </Button>
                        <Button className="flex items-center justify-center gap-2">
                            <HandshakeIcon size={18} />
                            Offer Draw
                        </Button>
                        <Button variant="destructive" className="flex items-center justify-center gap-2">
                            <Flag size={18} />
                            Resign
                        </Button>
                    </div>
                </div> */}
            </div>
        </div>
    );
}