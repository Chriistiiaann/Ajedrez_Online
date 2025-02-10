"use client";

import { useWebsocketContext } from "@/contexts/webContext-Context";
import { Badge } from "@/components/ui/badge";
import { UserPlus } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { useEffect } from "react";

interface InvitationBadgeProps {
  userId: string; // Recibir userId como prop
}

export default function InvitationBadge({ userId }: InvitationBadgeProps) {
    const { sendMessage, screenMessages } = useWebsocketContext();
    const { toast } = useToast();

    // Monitorear el arreglo de mensajes para usar el último recibido
    useEffect(() => {
        const messagesContent = screenMessages;
            console.log("contenido del arreglo de mensajes:", messagesContent);
        if (messagesContent.length > 0) {
        // Obtenemos el último mensaje del arreglo
            const lastMessage = messagesContent[messagesContent.length - 1].Message;
            console.log("ultimo mensaje:",lastMessage);
        // Verificamos la propiedad que indica el estado de la solicitud
        if (lastMessage === "Ya existe una solicitud pendiente o no puedes enviarla a ti mismo.") {
            toast({
            variant: "destructive",
            title: "Solicitud ya enviada ☹️",
            description: "Ya has enviado una solicitud de amistad a este usuario.",
            duration: 3000,
            });
        } else if (lastMessage === "sent") {
            toast({
            variant: "success",
            title: "Solicitud enviada ✅",
            description: "La solicitud de amistad se ha enviado correctamente.",
            duration: 3000,
            });
        } else {
            console.log("No se ha enviado ninguna solicitud de amistad.");
        }
        }
    }, [screenMessages, toast]);

    const handleClick = () => {
        // Enviamos el mensaje por el WebSocket
        sendMessage("sendFriendRequest", userId);
        console.log(`Solicitud de amistad enviada a usuario ${userId}`);
        // Opcional: puedes mostrar un toast inmediato de "Enviando solicitud..." mientras esperas la respuesta
        // toast({ title: "Enviando solicitud...", duration: 1500 });
    };

    return (
        <Badge variant="outline" onClick={handleClick}>
        <UserPlus className="h-5 w-5 cursor-pointer text-muted-foreground hover:text-primary" />
        </Badge>
    );
}
