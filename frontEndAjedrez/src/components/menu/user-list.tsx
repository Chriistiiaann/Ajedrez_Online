'use client'

import { useState, useEffect } from 'react'
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { UserRoundPlus } from 'lucide-react'
import { Button } from '../ui/button'
import StateBadge from './state-badge' 
import { getAuth } from '@/actions/get-auth'
import {UserSearchModal} from './user-search-modal'

type Friend = {
    id: string;
    nickName: string;
    email: string;
    avatar: string;
};

export default function FriendsList() {
    const [searchTerm, setSearchTerm] = useState('')
    const [friends, setFriends] = useState<Friend[]>([])
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [isModalOpen, setIsModalOpen] = useState(false)
    const [emptyListMessage, setEmptyListMessage] = useState('')

    useEffect(() => {
        

        const fetchFriends = async () => {
            setLoading(true)
            setError(null)
            
            try {
                const authData = await getAuth();
                console.log(authData.decodedToken?.Id)
                const response = await fetch('https://localhost:7218/api/SearchFriends', { 
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ query: searchTerm, userId: authData.decodedToken?.Id}),
                });

                if (!response.ok) throw new Error('Error al obtener los amigos');

                const result = await response.json();
                setFriends(result.users); 

                if (result.users.length === 0) {

                    setEmptyListMessage('No se encontró ningún amigo con ese nombre');

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

    return (
        <div className="bg-foreground border p-4 rounded-lg shadow space-y-4">
            <h2 className="text-xl font-semibold">Amigos</h2>
            <div className='flex items-center gap-2'>
                <Input
                    className='bg-background'
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
            {error && <p className="text-md text-gray-500">{error}</p>}
            {emptyListMessage && <p className="text-md text-gray-500">{emptyListMessage}</p>}
            
            <ul className="space-y-2">
                {friends.length > 0 ? (
                    friends.map(friend => (
                        <li 
                            key={friend.id} 
                            className="flex justify-between items-center p-2 rounded-md"
                        > 
                            <div className="flex items-center space-x-2">
                                <Avatar>
                                    <AvatarImage src={'https://localhost:7218/' + friend.avatar} alt={friend.nickName} />
                                    <AvatarFallback>{friend.nickName.slice(0, 2).toUpperCase()}</AvatarFallback>
                                </Avatar>
                                <span>{friend.nickName}</span>
                            </div>

                            {/* -------------------- Status de prueba, cambiar por lo devuelto por el WebSocket ---------------------- */}
                            <StateBadge status="connected" /> 
                        </li>
                    ))
                ) : (
                    !loading && !error && searchTerm !== '' && <p className="text-sm text-gray-500">No se encontraron amigos</p>
                )}
            </ul>
        </div>
    )
}
