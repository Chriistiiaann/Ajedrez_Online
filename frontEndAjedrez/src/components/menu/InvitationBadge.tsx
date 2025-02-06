import { useWebsocketContext } from "@/contexts/webContext-Context";
import { Badge } from "@/components/ui/badge";
import { Mail } from "lucide-react";

interface InvitationBadgeProps {
    userId: string; // Recibir userId como prop
}

export default function InvitationBadge({ userId }: InvitationBadgeProps) {
    const { sendMessage } = useWebsocketContext();

    const handleClick = () => {
        // Solo enviamos dos parámetros: 'action' e 'id'
        sendMessage("sendFriendRequest", userId); 
        console.log(`Solicitud de amistad enviada a usuario ${userId}`);
    };

    return (
        <Badge
            variant="default"
            className="flex items-center gap-1 cursor-pointer hover:bg-primary transition"
            onClick={handleClick}
        >
            <Mail className="w-4 h-4" />
            <span>Solicitud de Amistad</span>
        </Badge>
    );
}


