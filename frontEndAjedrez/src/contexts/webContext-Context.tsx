"use client";
import { usePathname } from "next/navigation";
import { createContext, useState, useContext, ReactNode, useEffect } from "react";
import { getAuth } from "@/actions/get-auth";

interface WebsocketContextType {
    socket: WebSocket | null;
    messages: Record<string, any>;
    screenMessages: Record<string, any>;
    sendMessage: (action: string, id?: string, position?: string) => Promise<void>;
    matchMakingState: string;
    matchMakingMessage: object;
    gameId: string;
    gameStatus: Record<string, any> | null;
    playerColor: string | null;
    chatMessages: Record<string, any>[];
    clearChatMessages: () => void;
    friendRequestsNumber: number;
    userData: { name: string; image: string } | null;
    opponentData: { name: string; image: string } | null;
    setUserData: (data: { name: string; image: string }) => void;
    setOpponentData: (data: { name: string; image: string }) => void;
    gameInvites: { gameId: string; senderNickname: string; senderAvatar: string }[];
    acceptGameInvite: (gameId: string) => void;
    rejectGameInvite: (gameId: string) => void;
    notYourTurn: boolean;
}

export const WebsocketContext = createContext<WebsocketContextType | undefined>(undefined);

export const useWebsocketContext = (): WebsocketContextType => {
    const context = useContext(WebsocketContext);
    if (!context) {
        throw new Error("useWebsocketContext debe usarse dentro de un WebsocketProvider");
    }
    return context;
};

interface WebsocketProviderProps {
    children: ReactNode;
}

export const WebsocketProvider = ({ children }: WebsocketProviderProps) => {
    const [socket, setSocket] = useState<WebSocket | null>(null);
    const [IdToken, setIdToken] = useState<number | null>(null);
    const [messages, setMessages] = useState<Record<string, any>[]>([]);
    const [screenMessages, setScreenMessages] = useState<Record<string, any>[]>([]);
    const [matchMakingState, setMatchMakingState] = useState<string>('');
    const [matchMakingMessage, setMatchMakingMessage] = useState<object>({});
    const [gameId, setGameId] = useState<string>('');
    const [gameStatus, setGameStatus] = useState<Record<string, any> | null>(null);
    const [playerColor, setPlayerColor] = useState<string | null>(null);
    const [chatMessages, setChatMessages] = useState<Record<string, any>[]>([]);
    const [friendRequestsNumber, setfriendRequestsNumber] = useState<number>(0);
    const [userData, setUserData] = useState<{ name: string; image: string } | null>(null);
    const [opponentData, setOpponentData] = useState<{ name: string; image: string } | null>(null);
    const [gameInvites, setGameInvites] = useState<{ gameId: string; senderNickname: string; senderAvatar: string }[]>([]);
    const [notYourTurn, setNotYourTurn] = useState<boolean>(false);

    const pathname = usePathname();

    const clearChatMessages = () => {
        setChatMessages([]);
        console.log("Chat vaciado para nueva partida");
    };

    const acceptGameInvite = (gameId: string) => {
        sendMessage("acceptMatchInvitation", gameId);
        setGameInvites((prev) => prev.filter((invite) => invite.gameId !== gameId));
    };

    const rejectGameInvite = (gameId: string) => {
        sendMessage("rejectMatchInvitation", gameId);
        setGameInvites((prev) => prev.filter((invite) => invite.gameId !== gameId));
    };

    useEffect(() => {
        async function LeerToken() {
            const authData = await getAuth();
            const idToken = authData?.decodedToken?.Id ?? 0;
            setIdToken(idToken);
        }
        LeerToken();
    }, [pathname]);

    useEffect(() => {
        if (!IdToken) {
            if (socket) {
                socket.close();
                setSocket(null);
                console.log("cerrando socket");
            }
            return;
        }

        if (socket) return;

        try {
            const ws = new WebSocket(`wss://localhost:7218/api/handler?userId=${IdToken}`);

            ws.onopen = () => {
                console.log("WebSocket conectado.");
                setSocket(ws);
            };

            ws.onmessage = (event: MessageEvent) => {
                console.log("Mensaje recibido del servidor (raw):", event.data);
                
                try {
                    const newMessage: Record<string, any> = JSON.parse(event.data);
                    console.log("Mensaje parseado:", newMessage);

                    if (newMessage.success !== undefined) {
                        console.log("Success flag:", newMessage.success);
                    }

                    console.log("Estado actual de matchMakingState:", matchMakingState);

                    if (newMessage.totalUsersConnected) {
                        setMessages((prevMessages) => ({
                            ...prevMessages,
                            ...newMessage,
                        }));
                    }

                    if (newMessage.totalMatches) {
                        setMessages((prevMessages) => ({
                            ...prevMessages,
                            ...newMessage,
                        }));
                    } 

                    if (newMessage.playersInMatches) {
                        setMessages((prevMessages) => ({
                            ...prevMessages,
                            ...newMessage,
                        }));
                    }

                    if (newMessage.Message) {
                        setScreenMessages([newMessage]);
                    }

                    // Condiciones específicas primero
                    if (newMessage.senderId) {
                        console.log("Condición senderId cumplida:", newMessage.senderId);
                        setMatchMakingState("found");
                        setMatchMakingMessage(newMessage);
                        setGameId(newMessage.gameId);
                        console.log("Partida encontrada con amigo - gameId:", newMessage.gameId);
                    } else if (newMessage.success === true && newMessage.opponentId) {
                        console.log("Success === true y opponentId presente:", newMessage.opponentId);
                        setMatchMakingState("found");
                        setMatchMakingMessage(newMessage);
                        setGameId(newMessage.gameId);
                        console.log("Partida encontrada - gameId:", newMessage.gameId);
                    } else if (newMessage.success === true && newMessage.opponent == "bot") {
                        console.log("Success === true y partida con bot");
                        setMatchMakingState("botMatch");
                        setMatchMakingMessage(newMessage);
                        setGameId(newMessage.gameId);
                        console.log("Partida con bot - gameId:", newMessage.gameId);
                    } else if (newMessage.searchingOpponent) {
                        console.log("Success !== true, estableciendo searching");
                        setMatchMakingState("searching");
                        setMatchMakingMessage(newMessage);
                    }

                    if (newMessage.playerColor) {
                        setPlayerColor(newMessage.playerColor);
                        console.log("playerColor:", newMessage.playerColor);
                    }

                    if (newMessage.validMoves) {
                        console.log("Mensaje de validMoves recibido:", newMessage);
                    }

                    if (newMessage.notYourTurn) {
                        setNotYourTurn(true);
                        console.log("notYourTurn:", newMessage.notYourTurn);
                    }

                    if (newMessage.success && (newMessage.status === "Check" || newMessage.status === "Checkmate")) {
                        setGameStatus(newMessage);
                        console.log("Estado del juego actualizado:", newMessage);
                    }

                    if (newMessage.gameChatMessage) {
                        setChatMessages((prevMessages) => [...prevMessages, newMessage]);
                        console.log("Mensaje de chat recibido:", newMessage);
                    }

                    if (newMessage.friendRequestSent) {
                        setfriendRequestsNumber((prevRequests) => prevRequests + 1);
                        console.log("Mensaje de solicitud de amistad recibido:", newMessage);
                    }

                    if (newMessage.gameInvitation && newMessage.gameId) {
                        console.log("Invitación de juego recibida:", newMessage);
                        setGameInvites((prev) => [...prev, { 
                            gameId: newMessage.gameId, 
                            senderNickname: newMessage.senderNickname, 
                            senderAvatar: newMessage.senderAvatar 
                        }]);
                    }

                    if (newMessage.gameInvitationAccepted === true && newMessage.gameId) {
                        setGameInvites((prev) => prev.filter((invite) => invite.gameId !== newMessage.gameId));
                        console.log("Invitación de juego aceptada:", newMessage);
                    } else if (newMessage.gameInvitationAccepted === false && newMessage.gameId) {
                        setGameInvites((prev) => prev.filter((invite) => invite.gameId !== newMessage.gameId));
                        console.log("Invitación de juego rechazada:", newMessage);
                    }
                } catch (error) {
                    console.error("Error al parsear mensaje:", error);
                }
            };

            ws.onclose = (event) => {
                console.log("WebSocket desconectado.");
                setSocket(null);
                console.log("⚡ WebSocket desconectado:", event);
                if (event.wasClean) {
                    console.log("✅ Conexión cerrada limpiamente.");
                } else {
                    console.warn("🚨 Conexión cerrada de manera inesperada.");
                }
                console.log(`📌 Código de cierre: ${event.code}, Razón: ${event.reason}`);
                if (!event.wasClean) {
                    console.warn("Conexión cerrada inesperadamente, intentado reconectar...");
                    setTimeout(() => {
                        console.log("Reintentando conexión WebSocket...");
                        setSocket(new WebSocket(`wss://localhost:7218/api/handler?userId=${IdToken}`));
                    }, 5000);
                }
            };

            ws.onerror = (error) => {
                console.error("❌ Error en WebSocket:", error);
            };

            const handleBeforeUnload = () => {
                console.log("Cerrando WebSocket antes de recargar la página.");
                ws.close();
            };

            window.addEventListener("beforeunload", handleBeforeUnload);
            return () => {
                console.log("cerrando websoket");
                ws.close();
                console.log(ws.readyState);
                window.removeEventListener("beforeunload", handleBeforeUnload);
            };
        } catch (error) {
            console.log("Error en WebSocket:", error);
        }
    }, [IdToken]);

    const sendMessage = async (action: string, id?: string, position?: string): Promise<void> => {
        if (socket && socket.readyState === WebSocket.OPEN) {
            const message: Record<string, any> = { action };
            const userId = String(id);
            if (action === "sendFriendRequest") {
                message.toUserId = userId;
            } else if (action === "acceptFriendRequest" || action === "rejectFriendRequest") {
                message.requestId = userId;
            } else if (action === "findRandomMatch") {
                console.log("Enviando solicitud de partida aleatoria...");
            } else if (action === "playWithBot") {
                console.log("Enviando solicitud de partida con bot...");
            } else if (action === "inviteFriendToGame") {
                message.friendId = userId;
            } else if (action === "getValidMoves") {
                message.gameId = gameId;
                message.position = position;
            } else if (action === "makeMove") {
                message.gameId = id;
                message.move = position;
            } else if (action === "sendChatMessage") {
                message.gameId = id;
                message.message = position;
            } else if (action === "acceptMatchInvitation" || action === "rejectMatchInvitation") {
                message.gameId = id;
            }
            socket.send(JSON.stringify(message));
            console.log("Mensaje enviado:", message);
        } else {
            throw new Error("No hay conexión WebSocket activa.");
        }
    };

    const contextValue: WebsocketContextType = {
        socket,
        messages,
        screenMessages,
        sendMessage,
        matchMakingState,
        matchMakingMessage,
        gameId,
        gameStatus,
        playerColor,
        chatMessages,
        clearChatMessages,
        friendRequestsNumber,
        userData,
        opponentData,
        setUserData,
        setOpponentData,
        gameInvites,
        acceptGameInvite,
        rejectGameInvite,
        notYourTurn
    };

    return <WebsocketContext.Provider value={contextValue}>{children}</WebsocketContext.Provider>;
};