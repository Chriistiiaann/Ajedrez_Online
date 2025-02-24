"use client"

import Image from "next/image"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";


type VsScreenProps = {
    gameMode: string | null
    opponentData: { name: string; image: string }
    userData: { name: string; image: string }
}

export default function VsScreen({ gameMode, opponentData, userData }: VsScreenProps) {

    // const authData = await getAuth();

    // In a real application, you'd fetch these details from a user profile or game state
    const userDetails = {
        name: userData.name,
        image: userData.image,
    }

    const opponentDetails = {
        name: opponentData.name,
        image: opponentData.image,
    }

    return (
        <div className="flex flex-col items-center justify-center text-white">
        <div className="flex items-center justify-center w-full max-w-4xl">
            <div className="flex flex-col items-center mr-8">
            <Image
                src={`https://localhost:7218/${userDetails.image}` || "/placeholder.svg"}
                alt={userDetails.name}
                width={200}
                height={200}
                className="rounded-full border-4 border-blue-500"
            />
            {/* <Avatar className="h-20 w-20 border-2 border-white">
                    <AvatarImage src={`https://localhost:7218/${userDetails.image}`} alt={userDetails.name} />
                    <AvatarFallback>
                    {userDetails.name.slice(0, 2).toUpperCase()}
                    </AvatarFallback>
            </Avatar> */}
            <h2 className="text-2xl font-bold mt-4">{userDetails.name}</h2>
            </div>
            <div className="text-6xl font-extrabold text-red-500 mx-8">VS</div>
            <div className="flex flex-col items-center ml-8">
            <Image 
                src={
                    gameMode === "computer"
                    ? opponentDetails.image || "/placeholder.svg"
                    : (`https://localhost:7218/${opponentDetails.image}`) || "/placeholder.svg"
                }
                alt={opponentDetails.name}
                width={200}
                height={200}
                className="rounded-full border-4 border-red-500"
            />
            <h2 className="text-2xl font-bold mt-4">{opponentDetails.name}</h2>
            </div>
        </div>
        <div className="mt-12 text-3xl font-bold animate-pulse">
            {gameMode === "random" && "Preparate para la batalla!"}
            {gameMode === "friend" && "Esperando amigo..."}
            {gameMode === "computer" && "Reta a la maquina!"}
        </div>
        </div>
    )
}

