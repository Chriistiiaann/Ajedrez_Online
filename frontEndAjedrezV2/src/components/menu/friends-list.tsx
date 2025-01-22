'use client'

import { useState } from 'react'
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { UserRoundPlus } from 'lucide-react'
import { Badge } from "@/components/ui/badge"
import { Button } from '../ui/button'

type Friend = {
    id: string
    name: string
    avatar: string
    status: 'conectado' | 'desconectado' | 'jugando'
}

const mockFriends: Friend[] = [
    { id: '1', name: 'Alice', avatar: '/placeholder.svg?height=40&width=40', status: 'conectado' },
    { id: '2', name: 'Bob', avatar: '/placeholder.svg?height=40&width=40', status: 'jugando' },
    { id: '3', name: 'Charlie', avatar: '/placeholder.svg?height=40&width=40', status: 'desconectado' },
]

export default function FriendsList() {
    const [searchTerm, setSearchTerm] = useState('')
    const [friends] = useState<Friend[]>(mockFriends)

    const filteredFriends = friends.filter(friend =>
        friend.name.toLowerCase().includes(searchTerm.toLowerCase())
    )

    return (
        <div className="bg-foreground border p-4 rounded-lg shadow space-y-4">
        <h2 className="text-xl font-semibold">Tus Amigos</h2>
        <div className='flex items-center gap-2'>
            <Input
                className='bg-background'
                type="search"
                placeholder="Buscar amigos..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
            />
            <Button variant="outline" size="icon" onClick={() => console.log('buscando nuevos amigos')}>
                <UserRoundPlus className="h-4 w-4" />
            </Button>
        </div>
        <ul className="space-y-2">
            {filteredFriends.map(friend => (
            <li key={friend.id} className="flex items-center space-x-2 p-2 hover:bg-accent rounded-md">
                <Avatar>
                <AvatarImage src={friend.avatar} alt={friend.name} />
                <AvatarFallback>{friend.name.slice(0, 2).toUpperCase()}</AvatarFallback>
                </Avatar>
                <span>{friend.name}</span>
                <Badge variant={friend.status === 'conectado' ? 'default' : friend.status === 'jugando' ? 'secondary' : 'outline'}>
                {friend.status}
                </Badge>
            </li>
            ))}
        </ul>
        </div>
    )
}

