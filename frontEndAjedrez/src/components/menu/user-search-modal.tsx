"use client"

import { useEffect, useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Avatar, AvatarFallback, AvatarImage } from "@radix-ui/react-avatar"
import { getAuth } from '@/actions/get-auth'
import InvitationBadge from "./InvitationBadge"

type User = {
    avatar: string
    id: string
    name: string
    email: string
}

export function UserSearchModal({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) {
    const [searchQuery, setSearchQuery] = useState("")
    const [users, setUsers] = useState<User[]>([])

    useEffect(() => {
        if (isOpen) {
        setSearchQuery("")
        const searchUsers = async () => {
            try {
                const authData = await getAuth();
                const response = await fetch("https://localhost:7218/api/SearchUsers", {
                    method: "POST",
                    headers: {
                    "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        query: "",
                        userId: authData.decodedToken?.Id,
                    }),
                });
            
                if (!response.ok) {
                    throw new Error(`Error HTTP: ${response.status}`);
                }
            
                const data = await response.json();
                setUsers(data.users);
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
                const authData = await getAuth();
                const response = await fetch("https://localhost:7218/api/SearchUsers", {
                    method: "POST",
                    headers: {
                    "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        query: searchQuery,
                        userId: authData.decodedToken?.Id,
                    }),
                });
            
                if (!response.ok) {
                    throw new Error(`Error HTTP: ${response.status}`);
                }
            
                const data = await response.json();
                setUsers(data.users);
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
            <DialogTitle>Buscar usuarios</DialogTitle>
            <DialogDescription>
                Busca usuarios para añadir como amigos. Haz clic en el icono más para enviar una solicitud de amistad.
            </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
            <Input placeholder="Buscar..." value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} />
            <ScrollArea className="h-[300px] rounded-md border p-4">
                {users.map((user) => (
                <div key={user.id} className="flex items-center justify-between py-2">
                    <div className="flex justify-between items-center p-2 gap-2 rounded-md">
                                <Avatar>
                                    <AvatarImage className="h-10 w-10 rounded-full" src={'https://localhost:7218/' + user.avatar} alt={user.name} />
                                    {/* <AvatarFallback>{user.name.slice(0, 2).toUpperCase()}</AvatarFallback> */}
                                </Avatar>
                        <p className="font-medium">{user.name}</p>
                        <p className="text-sm text-muted-foreground">{user.email}</p>
                    </div>
                    <InvitationBadge userId={user.id} />
                </div>
                ))}
                {users.length === 0 && <p className="text-sm text-muted-foreground">No se encontraron usuarios</p>}
            </ScrollArea>
            </div>
        </DialogContent>
        </Dialog>
    )
}

