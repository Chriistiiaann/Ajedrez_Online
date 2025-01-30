'use client'

import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Inbox } from 'lucide-react'

type FriendRequest = {
    id: string
    name: string
    avatar: string
}

const mockFriendRequests: FriendRequest[] = [
    { id: '1', name: 'David', avatar: '/placeholder.svg?height=40&width=40' },
    { id: '2', name: 'Eva', avatar: '/placeholder.svg?height=40&width=40' },
]

export default function InboxButton() {
    const [isOpen, setIsOpen] = useState(false)
    const [friendRequests] = useState<FriendRequest[]>(mockFriendRequests)

    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen} >
        <DialogTrigger asChild>
            <Button variant="outline" className="w-full bg-foreground">
            <Inbox className="mr-2 h-4 w-4" />
            Bandeja de entrada ({friendRequests.length})
            </Button>
        </DialogTrigger>
        <DialogContent className='text-white'>
            <DialogHeader>
            <DialogTitle >Solicitudes de amistad</DialogTitle>
            </DialogHeader>
            <ul className="space-y-2">
            {friendRequests.map(request => (
                <li key={request.id} className="flex items-center justify-between p-2 hover:bg-accent rounded-md">
                <div className="flex items-center space-x-2">
                    <img src={request.avatar || "/placeholder.svg"} alt={request.name} className="w-8 h-8 rounded-full" />
                    <span>{request.name}</span>
                </div>
                <div>
                    <Button size="sm" className="mr-2">Aceptar</Button>
                    <Button size="sm" variant="outline">Rechazar</Button>
                </div>
                </li>
            ))}
            </ul>
        </DialogContent>
        </Dialog>
    )
}

