'use client'

import { useState, useEffect } from 'react'
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { UserRoundPlus } from 'lucide-react'
import { Button } from '../ui/button'
import InvitationBadge from './InvitationBadge' 
import { getAuth } from '@/actions/get-auth'

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

    useEffect(() => {
        // if (searchTerm.trim() === '') {
        //     setFriends([]) 
        //     return
        // }

        const fetchFriends = async () => {
            setLoading(true)
            setError(null)
            
            try {
                const authData = await getAuth();
                console.log(authData.decodedToken?.Id)
                const response = await fetch('https://localhost:7218/api/SearchUsers', { 
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ query: searchTerm, userId: authData.decodedToken?.Id}),
                });

                if (!response.ok) throw new Error('Error al buscar Usuarios');

                const result = await response.json();
                setFriends(result.users); 
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
            <h2 className="text-xl font-semibold">Usuarios</h2>
            <div className='flex items-center gap-2'>
                <Input
                    className='bg-background'
                    type="search"
                    placeholder="Buscar usuarios..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                />
                <Button variant="outline" size="icon" onClick={() => console.log('buscando nuevos amigos')}>
                    <UserRoundPlus className="h-4 w-4" />
                </Button>
            </div>

            {loading && <p className="text-sm text-gray-500">Cargando...</p>}
            {error && <p className="text-sm text-red-500">{error}</p>}
            
            <ul className="space-y-2">
                {friends.length > 0 ? (
                    friends.map(friend => (
                        <li 
                            key={friend.id} 
                            className="flex justify-between items-center p-2 hover:bg-accent rounded-md"
                        > 
                            <div className="flex items-center space-x-2">
                                <Avatar>
                                    <AvatarImage src={'https://localhost:7218/' + friend.avatar} alt={friend.nickName} />
                                    <AvatarFallback>{friend.nickName.slice(0, 2).toUpperCase()}</AvatarFallback>
                                </Avatar>
                                <span>{friend.nickName}</span>
                            </div>

                            <InvitationBadge /> 
                        </li>
                    ))
                ) : (
                    !loading && !error && searchTerm !== '' && <p className="text-sm text-gray-500">No se encontraron amigos</p>
                )}
            </ul>
        </div>
    )
}
