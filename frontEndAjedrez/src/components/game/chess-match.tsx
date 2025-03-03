"use client";

import { useState, useCallback, useEffect } from "react";
import { Chess } from "chess.js";
import { Send, RotateCcw, Plus, Flag, HandshakeIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import ChessBoard from "./chess-board";
import { useWebsocketContext } from "@/contexts/webContext-Context";

export default function ChessMatch({ gameId }: { gameId: string }) {
    const [game, setGame] = useState(new Chess());
    const [newMessage, setNewMessage] = useState("");
    const { sendMessage, chatMessages, clearChatMessages } = useWebsocketContext();

    console.log(gameId);

    useEffect(() => {
        clearChatMessages();
    }, [gameId, clearChatMessages]);

    const sendMessageHandler = useCallback(
        (message: string) => {
            if (message.trim() === "") return;
            sendMessage("sendChatMessage", gameId, message.trim());
            setNewMessage("");
        },
        [sendMessage, gameId]
    );

    function resetGame() {
        setGame(new Chess());
    }

    function undoMove() {
        const newGame = new Chess(game.fen());
        newGame.undo();
        setGame(newGame);
    }

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
                            <ChessBoard />
                        </div>
                    </div>
                </div>

                <div className="lg:w-1/3 flex flex-col bg-foreground shadow-md rounded-lg overflow-hidden h-[500px] lg:h-auto">
                    <div className="p-3 bg-accent border-b">
                        <h2 className="font-semibold">Game Chat</h2>
                    </div>

                    <div className="flex-1 overflow-y-auto p-4 space-y-3">
                        {chatMessages.map((msg, index) => (
                            <div key={index} className={`flex ${msg.sender === "Player 1" ? "justify-end" : "justify-start"}`}>
                                <div className={`max-w-[80%] rounded-lg px-3 py-2 ${msg.sender === "Player 1" ? "bg-blue-400" : "bg-gray-100"}`}>
                                    <p className="text-xs text-gray-500">{msg.sender}</p>
                                    <p>{msg.message}</p>
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
                                onKeyDown={(e) => e.key === "Enter" && sendMessageHandler(newMessage.trim())}
                            />
                            <button
                                onClick={() => sendMessage("sendChatMessage", gameId, newMessage.trim())}
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
                    <h2 className="text-xl font-semibold mb-2">Move History</h2>
                    <div className="h-48 overflow-y-auto">
                        {/* Aquí puedes integrar moveHistory si lo reactivas */}
                    </div>
                </div>

                <div className="bg-foreground shadow-md rounded-lg p-4">
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
                </div>
            </div>
        </div>
    )
}