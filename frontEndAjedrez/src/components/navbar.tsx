'use client'

import Link from 'next/link'
import { Button } from "@/components/ui/button"
import { CastleIcon as ChessKnight } from 'lucide-react'

export default function Navbar() {

    return (
        <nav className="bg-foreground border-b text-white">
        <div className="container mx-auto px-4">
            <div className="flex items-center justify-between h-16">
            <div className="flex items-center">
                <Link href="/" className="flex items-center space-x-2">
                <ChessKnight className="h-8 w-8" />
                <span className="text-xl font-bold">Scacchi</span>
                </Link>
            </div>
            <div className="hidden md:flex items-center space-x-4">
                <Link href="/reglas" className="text-gray-500/80 hover:text-secondary">
                    Guía
                </Link>
                <Link href="/login" className="text-black ">
                    <Button variant="outline" className='bg-primary'>Iniciar Sesion</Button>
                </Link>
                
            </div>
            <div className="md:hidden">
                <Button variant="ghost" size="icon" className="text-white">
                <span className="sr-only">Toggle Menu</span>
                <svg
                    className="h-6 w-6"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M4 6h16M4 12h16M4 18h16"
                    />
                </svg>
                </Button>
            </div>
            </div>
        </div>
        </nav>
    )
}

