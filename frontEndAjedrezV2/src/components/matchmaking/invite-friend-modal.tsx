"use client"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

type InviteFriendModalProps = {
    isOpen: boolean
    onClose: () => void
    onInviteSent: () => void
}

export default function InviteFriendModal({ isOpen, onClose, onInviteSent }: InviteFriendModalProps) {
    const [friendUsername, setFriendUsername] = useState("")

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        // Here you would typically send the invite to the backend
        console.log(`Inviting friend: ${friendUsername}`)
        onInviteSent()
    }

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent className="sm:max-w-[425px] text-white">
            <DialogHeader>
            <DialogTitle>Invita a un amigo</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit}>
            <div className="grid gap-4 py-4">
                <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="username" className="text-right">
                    Nombre de usuario
                </Label>
                <Input
                    id="username"
                    value={friendUsername}
                    onChange={(e) => setFriendUsername(e.target.value)}
                    className="col-span-3"
                    placeholder="Introduce el nombre de tu amigo"
                />
                </div>
            </div>
            <DialogFooter>
                <Button type="submit" disabled={!friendUsername}>
                Mandar invitación
                </Button>
            </DialogFooter>
            </form>
        </DialogContent>
        </Dialog>
    )
}

