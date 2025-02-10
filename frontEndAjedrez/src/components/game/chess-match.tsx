"use client"

import { useState, useCallback, useEffect } from "react"
import { Chess } from "chess.js"
import { Chessboard } from "react-chessboard"
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
        <h1 className="text-3xl font-bold mb-6 text-center">Chess Match</h1>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="md:col-span-2">
            {/* <Chessboard position={game.fen()} onPieceDrop={onDrop} /> */}
            <ChessBoard />
            </div>
            <div className="space-y-6">
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Players</h2>
                <div className="space-y-2">
                <p>
                    <span className="font-medium">White:</span> Player 1
                </p>
                <p>
                    <span className="font-medium">Black:</span> Player 2
                </p>
                </div>
            </div>
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Game Controls</h2>
                <div className="space-y-2">
                <Button onClick={resetGame} className="w-full bg-accent">
                    New Game
                </Button>
                <Button onClick={undoMove} className="w-full bg-primary">
                    Undo Move
                </Button>
                <Button className="w-full bg-secondary">Offer Draw</Button>
                <Button variant="destructive" className="w-full">
                    Resign
                </Button>
                </div>
            </div>
            <div className="bg-foreground shadow rounded-lg p-4">
                <h2 className="text-xl font-semibold mb-2">Move History</h2>
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

