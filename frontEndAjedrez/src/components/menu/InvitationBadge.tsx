import { Mail } from "lucide-react";
import { Badge } from "@/components/ui/badge";


export default function InvitationBadge() {
    return (
        <Badge 
            variant="default" 
            className="flex items-center gap-1 cursor-pointer hover:bg-primary transition"
        >
            <Mail className="w-4 h-4" /> 
            <span>Solicitud de Amistad</span>
        </Badge>
    );
}
