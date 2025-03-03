"use client";

import { useState } from "react";
import { Bell } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { useWebsocketContext } from "@/contexts/webContext-Context";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

export default function GameInvitations() {
    const { gameInvites, acceptGameInvite, rejectGameInvite } = useWebsocketContext();
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleOpenModal = () => setIsModalOpen(true);
    const handleCloseModal = () => setIsModalOpen(false);

    const handleAccept = (gameId: string) => {
        acceptGameInvite(gameId);
    };

    const handleReject = (gameId: string) => {
        rejectGameInvite(gameId);
    };

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