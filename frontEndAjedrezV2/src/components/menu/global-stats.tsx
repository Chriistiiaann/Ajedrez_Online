import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

async function getGlobalStats() {
  // In a real application, you would fetch this data from an API
  // For this example, we'll use mock data
    return {
        connectedPlayers: 10532,
        activeMatches: 2145,
        playersInMatch: 4290
    }
}

export default async function GlobalStats() {
    const stats = await getGlobalStats()

    return (
        <Card>
        <CardHeader>
            <CardTitle>Global Statistics</CardTitle>
        </CardHeader>
        <CardContent className="space-y-2">
            <p>Connected Players: {stats.connectedPlayers}</p>
            <p>Active Matches: {stats.activeMatches}</p>
            <p>Players in Match: {stats.playersInMatch}</p>
        </CardContent>
        </Card>
    )
}

