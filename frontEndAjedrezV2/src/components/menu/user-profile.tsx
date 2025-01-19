'use client'

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"


export default function UserProfile() {
  // Assume we get the user data from a context or prop
    const user = {
        name: "ChessMaster2000",
        avatar: "/placeholder.svg?height=40&width=40"
    }

    return (
        <div className="flex items-center justify-between bg-card p-4 rounded-lg shadow">
        <div className="flex items-center space-x-4">
            <Avatar>
            <AvatarImage src={user.avatar} alt={user.name} />
            <AvatarFallback>{user.name.slice(0, 2).toUpperCase()}</AvatarFallback>
            </Avatar>
            <span className="font-semibold">{user.name}</span>
        </div>
        <Button variant="outline" onClick={ () => {console.log('logout')}}>
            Logout
        </Button>
        </div>
    )
}

