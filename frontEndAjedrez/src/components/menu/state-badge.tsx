
import { Badge } from "@/components/ui/badge";


export default function StateBadge({ status }: { status: string }) {
    return (() => {
        switch (status) {
            case "connected": return <Badge variant="default" className="flex items-center gap-1 cursor-pointer hover:bg-primary transition">Conectado</Badge>;
            case "disconnected": return <Badge variant="destructive" className="flex items-center gap-1 cursor-pointer hover:bg-destructive transition">Desconectado</Badge>;
            case "playing": return <Badge variant="secondary" className="flex items-center gap-1 cursor-pointer hover:bg-secondary transition">Jugando</Badge>;

            default: return <Badge variant="default" className="flex items-center gap-1 cursor-pointer hover:bg-primary transition">Desconectado</Badge>;
        }
    })();
}
