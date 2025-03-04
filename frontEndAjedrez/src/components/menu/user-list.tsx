"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { UserRoundPlus, UserRoundX } from "lucide-react";
import { Button } from "../ui/button";
import StateBadge from "./state-badge";
import { getAuth } from "@/actions/get-auth";
import { UserSearchModal } from "./user-search-modal";
import { useUserContext } from "@/contexts/user-context";

type Friend = {
    id: string;
    nickName: string;
    email: string;
    avatar: string;
};

export default function FriendsList() {
    const [searchTerm, setSearchTerm] = useState("");
    const [friends, setFriends] = useState<Friend[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [emptyListMessage, setEmptyListMessage] = useState("");
    const { userDataContext } = useUserContext();

    console.log("userDataContext:", userDataContext);

    useEffect(() => {
        const fetchFriends = async () => {
            setLoading(true);
            setError(null);
            
            try {
                const authData = await getAuth();
                const response = await fetch("https://localhost:7218/api/SearchFriends", { 
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ query: searchTerm, userId: authData.decodedToken?.Id }),
                });

                if (!response.ok) throw new Error("Error al obtener los amigos");

                const result = await response.json();
                const users = Array.isArray(result.users) ? result.users : [];
                setFriends(users); 

                if (users.length === 0) {
                    setEmptyListMessage("No se encontró ningún amigo con ese nombre");
                }
            } catch (err: any) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        const debounceTimeout = setTimeout(() => {
            fetchFriends();
        }, 500);

        return () => clearTimeout(debounceTimeout);
    }, [searchTerm]);

    const deleteFriend = async (friendId: string) => {
        if (!userDataContext?.user.Id) {
            setError("No se pudo identificar al usuario actual.");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const response = await fetch(`https://localhost:7218/api/Friend/${friendId}?userId=${userDataContext.user.Id}`, {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                    // Si el endpoint requiere autenticación, podrías añadir el token aquí
                    // "Authorization": `Bearer ${authData.token.accessToken}`,
                },
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || "Error al eliminar al amigo");
            }

            // Actualizar la lista de amigos eliminando al amigo localmente
            setFriends((prevFriends) => prevFriends.filter((friend) => friend.id !== friendId));
            console.log(`Amigo con ID ${friendId} eliminado exitosamente`);
        } catch (err: any) {
            setError(err.message);
            console.error("Error al eliminar amigo:", err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="bg-foreground border p-4 rounded-lg shadow space-y-4">
            <h2 className="text-xl font-semibold">Amigos</h2>
            <div className="flex items-center gap-2">
                <Input
                    className="bg-background"
                    type="search"
                    placeholder="Busca entre tus amigos..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                />
                <Button variant="outline" size="icon" onClick={() => setIsModalOpen(true)}>
                    <UserRoundPlus className="h-4 w-4" />
                </Button>
                <UserSearchModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} />
            </div>

            {loading && <p className="text-sm text-gray-500">Cargando...</p>}
            {error && <p className="text-md text-red-500">{error}</p>}
            {emptyListMessage && !friends.length && !loading && !error && (
                <p className="text-md text-gray-500">{emptyListMessage}</p>
            )}
            
            <ul className="space-y-2">
                {friends.length > 0 ? (
                    friends.map((friend) => (
                        <li 
                            key={friend.id} 
                            className="flex justify-between items-center p-2 rounded-md"
                        > 
                            <div className="flex items-center space-x-2">
                                <Avatar>
                                    <AvatarImage src={"https://localhost:7218/" + friend.avatar} alt={friend.nickName} />
                                    <AvatarFallback>{friend.nickName.slice(0, 2).toUpperCase()}</AvatarFallback>
                                </Avatar>
                                <span>{friend.nickName}</span>
                            </div>
                            <div className="flex items-center space-x-2">
                                <StateBadge status="connected" />
                                <Button 
                                    variant="destructive" 
                                    size="icon"
                                    onClick={() => deleteFriend(friend.id)}
                                    disabled={loading}
                                >
                                    <UserRoundX className="h-4 w-4" />
                                </Button>
                            </div>
                        </li>
                    ))
                ) : (
                    !loading && !error && searchTerm !== "" && <p className="text-sm text-gray-500">No se encontraron amigos</p>
                )}
            </ul>
        </div>
    );
}