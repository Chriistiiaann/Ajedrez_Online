"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Users, UserPlus, Cpu } from "lucide-react"
import VsScreen from "./vs-creen"
import InviteFriendModal from "./invite-friend-modal"

interface MatchmakingOptionsProps {
    onGameModeSelect: () => void
}

export default function MatchmakingOptions({ onGameModeSelect }: MatchmakingOptionsProps) {
    const [selectedOption, setSelectedOption] = useState<string | null>(null)
    const [showVsScreen, setShowVsScreen] = useState(false)
    const [showInviteModal, setShowInviteModal] = useState(false)

    const handleOptionClick = (option: string) => {
        setSelectedOption(option)
        onGameModeSelect()
        if (option === "friend") {
        setShowInviteModal(true)
        } else {
        // Simulate a delay before showing the VS screen
        setTimeout(() => setShowVsScreen(true), 1500)
        }
    }

    const handleInviteSent = () => {
        setShowInviteModal(false)
        // Simulate waiting for friend to join
        setTimeout(() => setShowVsScreen(true), 2000)
    }

    if (showVsScreen) {
        return <VsScreen gameMode={selectedOption} />
    }

    return (
        <div className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Button
            variant="outline"
            size="lg"
            className="h-32 flex flex-col items-center justify-center"
            onClick={() => handleOptionClick("random")}
            >
            <Users className="h-8 w-8 mb-2" />
            <span>Jugar Online</span>
            <span className="text-sm text-muted-foreground">(Jugador aleatorio)</span>
            </Button>
            <Button
            variant="outline"
            size="lg"
            className="h-32 flex flex-col items-center justify-center"
            onClick={() => handleOptionClick("friend")}
            >
            <UserPlus className="h-8 w-8 mb-2" />
            <span>Jugar contra un Amigo</span>
            </Button>
            <Button
            variant="outline"
            size="lg"
            className="h-32 flex flex-col items-center justify-center"
            onClick={() => handleOptionClick("computer")}
            >
            <Cpu className="h-8 w-8 mb-2" />
            <span>Jugar contra la maquina</span>
            </Button>
        </div>
        {selectedOption && !showVsScreen && (
            <p className="text-center text-lg mt-8">
            {selectedOption === "random" && "Buscando un oponente aleatorio..."}
            {selectedOption === "friend" && "¡Invita a un amigo!"}
            {selectedOption === "computer" && "Preparando juego contra la maquina..."}
            </p>
        )}
        <InviteFriendModal
            isOpen={showInviteModal}
            onClose={() => setShowInviteModal(false)}
            onInviteSent={handleInviteSent}
        />
        </div>
    )
}

