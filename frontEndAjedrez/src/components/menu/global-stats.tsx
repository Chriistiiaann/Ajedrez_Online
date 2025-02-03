import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Users, Swords, UserCheck } from "lucide-react"

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

    return (
        <Card className="bg-foreground">
        <CardHeader>
            <CardTitle className="text-xl font-bold">Estadisticas Globales</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
            <div className="flex items-center space-x-4">
            <Users className="h-6 w-6 text-blue-500" />
            <div>
                <p className="text-sm font-medium">Connected Players</p>
                <p className="text-2xl font-bold">{stats.connectedPlayers.toLocaleString()}</p>
            </div>
            </div>
            <div className="flex items-center space-x-4">
            <Swords className="h-6 w-6 text-red-500" />
            <div>
                <p className="text-sm font-medium">Active Matches</p>
                <p className="text-2xl font-bold">{stats.activeMatches.toLocaleString()}</p>
            </div>
            </div>
            <div className="flex items-center space-x-4">
            <UserCheck className="h-6 w-6 text-green-500" />
            <div>
                <p className="text-sm font-medium">Players in Match</p>
                <p className="text-2xl font-bold">{stats.playersInMatch.toLocaleString()}</p>
            </div>
            </div>
        </CardContent>
        </Card>
    )
}

