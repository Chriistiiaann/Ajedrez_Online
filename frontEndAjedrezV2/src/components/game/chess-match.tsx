"use client"

import { useState, useCallback, useEffect } from "react"
import { Chess } from "chess.js"
import ChessBoard from "@/components/game/chess-board"
import { Button } from "@/components/ui/button"

export default function ChessMatch() {
    const [game, setGame] = useState(new Chess())
    const [moveHistory, setMoveHistory] = useState<string[]>([])

    useEffect(() => {
        setMoveHistory(game.history())
    }, [game])

    const makeAMove = useCallback(
        (move: any) => {
        try {
            const result = game.move(move)
            setGame(new Chess(game.fen()))
            return result
        } catch (error) {
            return null
        }
        },
        [game],
    )

    function onDrop(sourceSquare: string, targetSquare: string) {
        const move = makeAMove({
        from: sourceSquare,
        to: targetSquare,
        promotion: "q", // always promote to a queen for example simplicity
        })

        if (move === null) return false
        return true
    }

    function resetGame() {
        setGame(new Chess())
    }

    function undoMove() {
        const newGame = new Chess(game.fen())
        newGame.undo()
        setGame(newGame)
    }

    return (
        <div className="container mx-auto px-4 py-8">
        {/* <h1 className="text-3xl font-bold mb-6 text-center">En Partida</h1> */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="md:col-span-2">
            {/* <Chessboard position={game.fen()} onPieceDrop={onDrop} /> */}
            <ChessBoard />
            </div>
            <div className="space-y-6">
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Jugadores</h2>
                <div className="space-y-2">
                <p>
                    <span className="font-medium">Blanco:</span> Jugador 1
                </p>
                <p>
                    <span className="font-medium">Negro:</span> Jugador 2
                </p>
                </div>
            </div>
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Opciones de juego</h2>
                <div className="space-y-2">
                <Button onClick={resetGame} className="w-full bg-accent">
                    Nuevo juego
                </Button>
                <Button onClick={undoMove} className="w-full bg-primary">
                    Deshacer movimiento
                </Button>
                <Button className="w-full bg-secondary">Ofrecer empate</Button>
                <Button variant="destructive" className="w-full">
                    Rendirse
                </Button>
                </div>
            </div>
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Historial de movimientos</h2>
                <div className="h-48 overflow-y-auto">
                {moveHistory.map((move, index) => (
                    <p key={index} className="text-sm">
                    {index % 2 === 0 ? `${Math.floor(index / 2) + 1}. ` : ""}
                    {move}
                    </p>
                ))}
                </div>
            </div>
            </div>
        </div>
        </div>
    )
}

