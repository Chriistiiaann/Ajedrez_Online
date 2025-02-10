import Image from 'next/image'
import bigQueen from "@/public/chess-piece-queen.png"

export default function Hero() {
    return (
        <div className="flex-grow relative h-screen w-auto bg-foreground overflow-hidden">
            {/* Background pattern */}
            <div className="absolute inset-0 bg-grid-white/[0.02] bg-[size:60px_60px]" />
            
            {/* Content container */}
            <div className="relative h-full w-full flex flex-col items-center justify-center px-4">
                {/* Chess piece decoration */}
                <div className="absolute right-[10%] top-1/2 -translate-y-1/2 opacity-20">
                <Image
                    src={bigQueen}
                    width={400}
                    height={400}
                    alt=""
                    className="w-[400px] h-[400px]"
                    priority
                />
                </div>
                
                {/* Main text */}
                <div className="relative z-10 text-center space-y-4">
                <h1 className="text-6xl md:text-8xl font-bold tracking-tighter">
                    <span className="block text-secondary">Chess</span>
                    <span className="block text-primary">Check Mates</span>
                </h1>
                <p className="text-gray-400 text-xl md:text-2xl max-w-2xl mx-auto">
                    Tu mejor opción para jugar ajedrez en línea.
                </p>
                </div>
            </div>
            
            {/* Gradient overlay */}
            <div className="absolute inset-0 bg-gradient-to-t from-background to-transparent" />
        </div>
    )
}

