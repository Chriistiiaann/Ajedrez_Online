import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Users, Swords, UserCheck } from "lucide-react"
import { useWebsocketContext } from '@/contexts/webContext-Context' 
import {useEffect, useState} from "react";

type WebSocketMessage = {
    totalUsersConnected: number;  
};

function getGlobalStats() {
  // In a real application, you would fetch this data from an API
  // For this example, we'll use mock data
    return {
        connectedPlayers: 10532,
        activeMatches: 2145,
        playersInMatch: 4290,
    }
}

export default  function GlobalStats() {
    const stats =  getGlobalStats()
    const {messages} = useWebsocketContext();
       const [totalUsersConnected, setTotalUsersConnected] = useState(0);
       const [totalActiveMatches, setTotalActiveMatches] = useState(0);
       const [totalPlayersInMatch, setTotalPlayersInMatch] = useState(0);
       useEffect(() =>{
        console.log("mensaje recibido", messages);
        if (messages.totalUsersConnected !=undefined ){
          setTotalUsersConnected(messages.totalUsersConnected);
          setTotalActiveMatches(messages.totalMatches);
          setTotalPlayersInMatch(messages.playersInMatches);
        }
       }, [messages])
       console.log(messages?.totalUsersConnected ?? "Cargando...");

    return (
        <Card className="bg-foreground">
        <CardHeader>
            <CardTitle className="text-xl font-bold">Estadisticas Globales</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
            <div className="flex items-center space-x-4">
            <UserCheck className="h-6 w-6 text-green-500" />
            <div>
                <p className="text-sm font-medium">Usuarios Conectados</p>
                <p className="text-2xl font-bold">{totalUsersConnected}</p>
            </div>
            </div>
            <div className="flex items-center space-x-4">
            <Swords className="h-6 w-6 text-red-500" />
            <div>
                <p className="text-sm font-medium">Partidas en curso</p>
                <p className="text-2xl font-bold">{totalActiveMatches}</p>
            </div>
            </div>
            <div className="flex items-center space-x-4">
            <Users className="h-6 w-6 text-blue-500" />
            <div>
                <p className="text-sm font-medium">Jugadores en partida</p>
                <p className="text-2xl font-bold">{totalPlayersInMatch}</p>
            </div>
            </div>
        </CardContent>
        </Card>
    )
}

