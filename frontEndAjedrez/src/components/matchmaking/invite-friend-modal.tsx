"use client"

import { useState, useEffect } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Avatar, AvatarFallback, AvatarImage } from "@radix-ui/react-avatar"
import InviteToMatchBadge from "./invite-to-match-badge"

type InviteFriendModalProps = {
    isOpen: boolean
    onClose: () => void
    onInviteSent: () => void
    userId: number
}

type Friend = {
    avatar: string
    id: string
    name: string
    email: string
}

export default function InviteFriendModal({ isOpen, onClose, onInviteSent, userId }: InviteFriendModalProps) {
    const [friends, setFriends] = useState<Friend[]>([])
    const [searchQuery, setSearchQuery] = useState("")

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        // Here you would typically send the invite to the backend
        console.log(`Inviting friend: ${userId}`)
        onInviteSent()
    }

    useEffect(() => {
            if (isOpen) {
            setSearchQuery("")
            const searchUsers = async () => {
                try {
                    const response = await fetch("https://localhost:7218/api/SearchFriends", {
                        method: "POST",
                        headers: {
                        "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            query: "",
                            userId: userId,
                        }),
                    });
                
                    if (!response.ok) {
                        throw new Error(`Error HTTP: ${response.status}`);
                    }
                
                    const data = await response.json();
                    setFriends(data.users);
                    console.log("Respuesta del servidor:", data);
                    } catch (error) {
                    console.error("Error en la petición:", error);
                    }
                };
                
                searchUsers();
                
            }
        }, [isOpen])
    
        useEffect(() => {
            const fetchUsers = async () => {
                try {
                    const response = await fetch("https://localhost:7218/api/SearchFriends", {
                        method: "POST",
                        headers: {
                        "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            query: searchQuery,
                            userId: userId,
                        }),
                    });
                
                    if (!response.ok) {
                        throw new Error(`Error HTTP: ${response.status}`);
                    }
                
                    const data = await response.json();
                    setFriends(data.users);
                    console.log("Respuesta del servidor:", data);
                    } catch (error) {
                    console.error("Error en la petición:", error);
                }
            };
            
                fetchUsers();
        }, [searchQuery])

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent className="sm:max-w-[425px] text-white">
            <DialogHeader>
            <DialogTitle>Invita a un amigo</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit}>
            <div className="grid gap-4 py-4">
                    <div className="grid gap-4 py-4">
                        <div className="">
                        <Input placeholder="Buscar..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} />
                        <ScrollArea className="h-[300px] rounded-md border p-4">
                            {friends.map((friend) => (
                            <div key={friend.id} className="flex items-center justify-between py-2">
                                <div className="flex justify-between items-center p-2 gap-2 rounded-md">
                                            <Avatar>
                                                <AvatarImage className="h-10 w-10 rounded-full" src={'https://localhost:7218/' + friend.avatar} alt={friend.name} />
                                                {/* <AvatarFallback>{friend.name.slice(0, 2).toUpperCase()}</AvatarFallback> */}
                                            </Avatar>
                                    <p className="font-medium">{friend.name}</p>
                                    <p className="text-sm text-muted-foreground">{friend.email}</p>
                                </div>
                                <InviteToMatchBadge friendId={friend.id} />
                            </div>
                            ))}
                            {friends.length === 0 && <p className="text-sm text-muted-foreground">No se encontraron usuarios</p>}
                        </ScrollArea>
                    </div>
                </div>
            </div>
            <DialogFooter>
                {/* <Button type="submit">
                Mandar invitación
                </Button> */}
            </DialogFooter>
            </form>
        </DialogContent>
        </Dialog>
    )
}

