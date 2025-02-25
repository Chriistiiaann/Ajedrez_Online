"use client";

import { useWebsocketContext } from "@/contexts/webContext-Context";
import { Badge } from "@/components/ui/badge";
import { MailPlus, CheckCircle, Loader2 } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { useState } from "react";

interface InvitationBadgeProps {
    friendId: string;
}

export default function InviteToMatchBadge({ friendId }: InvitationBadgeProps) {
    const { sendMessage, matchMakingState, matchMakingMessage } = useWebsocketContext();
    const { toast } = useToast();
    const [isConfirmed, setIsConfirmed] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleClick = () => {
        if (isConfirmed || isLoading) return;

        setIsLoading(true);
        sendMessage("inviteFriendToGame", friendId);
        console.log(`Solicitud de partida enviada a usuario con ID: ${friendId}`);

        toast({
            title: "Enviando invitación a partida 🕹️...",
            description: "La invitación se ha enviado correctamente.",
            variant: "success",
            duration: 3500,
        });

        setIsConfirmed(true);
    };



    return (
        <>
            <div className="relative inline-flex items-center">
                <Badge variant="outline" onClick={handleClick} className="cursor-pointer">
                    {isConfirmed ? (
                        <CheckCircle className="h-5 w-5 text-green-500" />
                    ) : (
                        <MailPlus className="h-5 w-5 text-muted-foreground hover:text-primary" />
                    )}
                </Badge>
                
            </div>
            {isLoading && (
                    <div className=" text-blue-500 border-1 border-gray-300 rounded-lg px-2 py-1 flex items-center text-sm bg-foreground shadow-md">
                        <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                        Esperando...
                    </div>
            )}
        </>
    );
}
