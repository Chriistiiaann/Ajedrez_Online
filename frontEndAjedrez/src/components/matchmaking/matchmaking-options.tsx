"use client"

import { useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Users, UserPlus, Cpu } from "lucide-react"
import VsScreen from "./vs-screen"
import InviteFriendModal from "./invite-friend-modal"
import { useWebsocketContext } from "@/contexts/webContext-Context";
import { useUserContext } from "@/contexts/user-context";

interface MatchmakingOptionsProps {
    onGameModeSelect: () => void
}

interface MatchMakingMessageType {
    opponentNickName: string;
    opponentAvatar: string;
}

interface MatchMakingMessageTypeFriend {
    senderNickname: string;
    senderAvatar: string;
}

export default function MatchmakingOptions({ onGameModeSelect }: MatchmakingOptionsProps) {
    const [selectedOption, setSelectedOption] = useState<string | null>(null)
    const [showVsScreen, setShowVsScreen] = useState(false)
    const [showInviteModal, setShowInviteModal] = useState(false)
    const { sendMessage, matchMakingState, matchMakingMessage, gameId } = useWebsocketContext();
    const { userDataContext } = useUserContext();

    const handleOptionClick = (option: string) => {
        setSelectedOption(option)
        onGameModeSelect()
        if (option === "friend") {
        setShowInviteModal(true)
        } else if(option === "random") {
        // Simulate a delay before showing the VS screen
            sendMessage("findRandomMatch");
            
        } else if(option === "computer") {
            sendMessage("playWithBot");
            setTimeout(() => setShowVsScreen(true), 3500)
        }
    }

    useEffect(() => {
        if(matchMakingState === "searching") {
            console.log("Buscando oponente...");
        } else if(matchMakingState === "found") {
            console.log("Oponente encontrado!");
            setTimeout(() => setShowVsScreen(true), 3500)
        } else if(matchMakingState === "botMatch") {
            console.log("Partida contra la maquina iniciada!");
            setTimeout(() => setShowVsScreen(true), 3500)
        }
    }, [matchMakingState])

    const handleInviteSent = () => {
        setShowInviteModal(false)
        // Simulate waiting for friend to join
        setTimeout(() => setShowVsScreen(true), 2000)
    }

    if (showVsScreen) {
        if (selectedOption === "computer") {
            console.log(userDataContext);
            return (
                <VsScreen
                gameMode={selectedOption}
                opponentData={{ name: "CPU", image: "/profilePics/botAvatar.png" }}
                userData={{ name: userDataContext?.user.NickName || "", image: userDataContext?.user.Avatar || "" }}
                gameId={gameId}
                />
            );
        
        } else if (selectedOption === "friend"){
            const message = matchMakingMessage as MatchMakingMessageTypeFriend;
                            return (
                                <VsScreen
                                gameMode="friend"
                                opponentData={{ name: message.senderNickname, image: message.senderAvatar }}
                                userData={{ name: userDataContext?.user.NickName || "", image: userDataContext?.user.Avatar || "" }}
                                gameId={gameId}
                                />
                            );
        } else {
            // Asumimos que matchMakingMessage es de tipo MatchMakingMessageType
            const message = matchMakingMessage as MatchMakingMessageType;
            
            // Verificamos que message tenga las propiedades esperadas
            if (!message || !message.opponentNickName || !message.opponentAvatar) {
                return <div>Cargando datos del oponente...</div>;
            }
            
            return (
                <VsScreen
                gameMode={selectedOption}
                opponentData={{ name: message.opponentNickName, image: message.opponentAvatar }}
                userData={{ name: userDataContext?.user.NickName || "", image: userDataContext?.user.Avatar || "" }}
                gameId={gameId}
                />
            );
        }
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
            userId={userDataContext?.user.Id || 0}
        />
        </div>
    )
}

