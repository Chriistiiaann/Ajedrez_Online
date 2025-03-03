"use client";

import { useState } from "react";
import { Bell } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { useWebsocketContext } from "@/contexts/webContext-Context";
import { send } from "process";

export default function GameInvitations() {
    const { sendMessage } = useWebsocketContext();
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleOpenModal = () => setIsModalOpen(true);
    const handleCloseModal = () => setIsModalOpen(false);

    const handleAccept = (gameId: string) => {
        sendMessage("acceptMatchInvitation", gameId);
    };

    const handleReject = (gameId: string) => {
        sendMessage("rejectMatchInvitation", gameId);
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
                    <span className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs">
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
                                <div key={invite.id} className="flex items-center justify-between p-2 bg-gray-800 rounded-lg">
                                    <span>{invite.senderName} te ha invitado a una partida</span>
                                    <div className="flex gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="bg-green-500 hover:bg-green-600"
                                            onClick={() => handleAccept(invite.id)}
                                        >
                                            Aceptar
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            className="bg-red-500 hover:bg-red-600"
                                            onClick={() => handleReject(invite.id)}
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