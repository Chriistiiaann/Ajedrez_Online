'use client'

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

import { Play, Zap } from 'lucide-react'
import Image from "next/image"

import Link from "next/link"

export function PlaySection() {
    // const [gameType, setGameType] = useState('quick')

    return (
        <Card className="bg-background overflow-hidden">
        <div className="relative">
            <Image
            src="/play-background.jpg"
            alt="Chess pieces"
            width={1200}
            height={400}
            className="object-cover w-full h-60"
            />
            <div className="absolute inset-0 bg-gradient-to-b from-transparent via-black/50 to-black" />
            <CardHeader className="absolute inset-x-0 top-0">
                <CardTitle className="text-2xl font-bold text-gray-200 bg-foreground w-fit p-2 rounded-md">
                    ¿Listo para jugar?
                </CardTitle>
            </CardHeader>
        </div>
        <CardContent className="relative pt-4">
            <div className="flex flex-wrap gap-4">
            <Button className="flex-1" onClick={() => ""}>
                <Play className="mr-2 h-4 w-4" /> vs Jugador
            </Button>
            <Button className="flex-1" variant="secondary">
                <Link href="/menu/juego" className="flex items-center">
                    <Zap className="mr-2 h-4 w-4" /> vs Computer
                </Link>
                
            </Button>
            </div>
        </CardContent>
    </Card>
    )
}

