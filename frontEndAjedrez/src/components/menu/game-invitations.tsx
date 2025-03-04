"use client";

import { useState, useEffect } from "react";
import { Bell } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { useWebsocketContext } from "@/contexts/webContext-Context";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useRouter } from "next/navigation";
import VsScreen from "@/components/matchmaking/vs-screen";
import { useUserContext } from "@/contexts/user-context";

export default function GameInvitations() {
    const { gameInvites, acceptGameInvite, rejectGameInvite, matchMakingState, gameId, matchMakingMessage } = useWebsocketContext();
    const { userDataContext } = useUserContext();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [showVsScreen, setShowVsScreen] = useState(false);
    const [acceptedGameId, setAcceptedGameId] = useState<string | null>(null);
    const router = useRouter();

    const handleOpenModal = () => setIsModalOpen(true);
    const handleCloseModal = () => setIsModalOpen(false);

    const handleAccept = (gameId: string) => {
        acceptGameInvite(gameId);
        setAcceptedGameId(gameId); // Guardamos el gameId aceptado
        setIsModalOpen(false); // Cerramos el modal inmediatamente
    };

    const handleReject = (gameId: string) => {
        rejectGameInvite(gameId);
    };

    // Detectar cuando la aceptación se confirma y mostrar VsScreen
    useEffect(() => {
        if (acceptedGameId && matchMakingState === "found" && gameId === acceptedGameId) {
            console.log("Invitación aceptada, mostrando VsScreen...");
            setShowVsScreen(true);

            // Redirigir al juego después de 3.5 segundos
            const timer = setTimeout(() => {
                setShowVsScreen(false);
                router.push(`/menu/juego?gameId=${gameId}`);
            }, 9000);

            return () => clearTimeout(timer); // Limpiar el temporizador al desmontar
        }
    }, [matchMakingState, gameId, acceptedGameId, router]);

    // Renderizar VsScreen en pantalla completa cuando corresponda
    if (showVsScreen && acceptedGameId) {
        const invite = gameInvites.find((inv) => inv.gameId === acceptedGameId);
        const message = matchMakingMessage as { senderNickname: string; senderAvatar: string };
        return (
            <div className="fixed inset-0 z-50 h-screen w-screen">
                <VsScreen
                    gameMode="friend"
                    opponentData={{ name: invite?.senderNickname || message.senderNickname, image: invite?.senderAvatar || message.senderAvatar }}
                    userData={{ name: userDataContext?.user.NickName || "", image: userDataContext?.user.Avatar || "" }}
                    gameId={gameId}
                />
            </div>
        );
    }

    return (
        <div className="relative">
            <Button
                variant="outline"
                className="flex items-center gap-2 bg-foreground text-white hover:bg-accent"
                onClick={handleOpenModal}
            >
                <Bell size={18} />
                <span>Invitaciones</span>
                {gameInvites.length > 0 && (
                    <span className="absolute -top-2 -left-1 bg-red-500 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs">
                        {gameInvites.length}
                    </span>
                )}
            </Button>

            <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
                <DialogContent className="sm:max-w-[425px] text-white" aria-describedby="dialog-description">
                    <DialogHeader>
                        <DialogTitle>Invitaciones de juego</DialogTitle>
                        <DialogDescription id="dialog-description">
                            Aquí puedes ver y gestionar las invitaciones de partidas recibidas.
                        </DialogDescription>
                    </DialogHeader>
                    <div className="mt-4 space-y-4">
                        {gameInvites.length === 0 ? (
                            <p className="text-gray-400">No hay invitaciones pendientes.</p>
                        ) : (
                            gameInvites.map((invite) => (
                                <div key={invite.gameId} className="flex items-center justify-between p-2 bg-foreground rounded-lg">
                                    <Avatar>
                                        <AvatarImage src={"https://localhost:7218/" + invite.senderAvatar} alt={invite.senderNickname} />
                                        <AvatarFallback>{invite.senderNickname.slice(0, 2).toUpperCase()}</AvatarFallback>
                                    </Avatar>
                                    <span>{invite.senderNickname} te ha invitado a una partida</span>
                                    <div className="flex gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="bg-background hover:bg-accent"
                                            onClick={() => handleAccept(invite.gameId)}
                                        >
                                            Aceptar
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="bg-background hover:bg-red-600"
                                            onClick={() => handleReject(invite.gameId)}
                                        >
                                            Rechazar
                                        </Button>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
}