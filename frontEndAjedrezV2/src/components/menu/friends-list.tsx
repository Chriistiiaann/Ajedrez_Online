'use client'

import { useState } from 'react'
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"

type Friend = {
    id: string
    name: string
    avatar: string
    status: 'connected' | 'disconnected' | 'playing'
}

const mockFriends: Friend[] = [
    { id: '1', name: 'Alice', avatar: '/placeholder.svg?height=40&width=40', status: 'connected' },
    { id: '2', name: 'Bob', avatar: '/placeholder.svg?height=40&width=40', status: 'playing' },
    { id: '3', name: 'Charlie', avatar: '/placeholder.svg?height=40&width=40', status: 'disconnected' },
]

export default function FriendsList() {
    const [searchTerm, setSearchTerm] = useState('')
    const [friends] = useState<Friend[]>(mockFriends)

    const filteredFriends = friends.filter(friend =>
        friend.name.toLowerCase().includes(searchTerm.toLowerCase())
    )

    return (
        <div className="bg-card p-4 rounded-lg shadow space-y-4">
        <h2 className="text-xl font-semibold">Friends</h2>
        <Input
            type="search"
            placeholder="Search friends..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
        />
        <ul className="space-y-2">
            {filteredFriends.map(friend => (
            <li key={friend.id} className="flex items-center space-x-2 p-2 hover:bg-accent rounded-md">
                <Avatar>
                <AvatarImage src={friend.avatar} alt={friend.name} />
                <AvatarFallback>{friend.name.slice(0, 2).toUpperCase()}</AvatarFallback>
                </Avatar>
                <span>{friend.name}</span>
                <Badge variant={friend.status === 'connected' ? 'default' : friend.status === 'playing' ? 'secondary' : 'outline'}>
                {friend.status}
                </Badge>
            </li>
            ))}
        </ul>
        </div>
    )
}

