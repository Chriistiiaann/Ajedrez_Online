"use client"

import ChessMatch from "@/components/game/chess-match"
import ChessBoard from "@/components/game/chess-board"

import { useSearchParams } from "next/navigation";

export default function VistaJuegoPage() { 

    const searchParams = useSearchParams();
    const gameId = searchParams.get("gameId");

    return (
        <>
            <ChessMatch gameId={gameId} />
        </>
    )
}