"use client"

import { useState } from "react"
import MatchmakingOptions from "@/components/matchmaking/matchmaking-options"

export default function Home() {
    const [gameModeSelected, setGameModeSelected] = useState(false)

    return (
        <main className="flex min-h-screen flex-col items-center justify-center p-4 sm:p-8 md:p-24">
        {!gameModeSelected && (
            <>
            <h1 className="text-4xl font-bold mb-8 text-center">Game Matchmaking</h1>
            <p className="text-xl mb-12 text-center max-w-2xl">
                Choose your preferred game mode. Play against random opponents online, challenge a friend, or test your
                skills against the computer!
            </p>
            </>
        )}
        <MatchmakingOptions onGameModeSelect={() => setGameModeSelected(true)} />
        </main>
    )
}

