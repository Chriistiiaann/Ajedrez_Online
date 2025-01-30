"use client"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import ImageUpload from "@/components/menu/image-upload"
import type { UserData } from "@/components/menu/user-profile"

interface UserEditModalProps {
    isOpen: boolean
    onClose: () => void
    onSave: (userData: UserData) => void
    initialData: UserData
}

export default function UserEditModal({ isOpen, onClose, onSave, initialData }: UserEditModalProps) {
    const [userData, setUserData] = useState<UserData>(initialData)

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setUserData((prev) => ({ ...prev, [name]: value }))
    }

    const handleImageChange = (image: string) => {
        setUserData((prev) => ({ ...prev, image }))
    }

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        onSave(userData)
        onClose()
    }

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent className="text-white sm:max-w-[425px]">
            <DialogHeader>
            <DialogTitle>Editar Perfil</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit}>
            <div className="grid gap-4 py-4">
                <ImageUpload currentImage={`https://localhost:7218/${userData.user.Avatar}`} onImageChange={handleImageChange} />
                <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="username" className="text-right">
                    Nombre
                </Label>
                <Input
                    id="username"
                    name="username"
                    value={userData.user.NickName}
                    onChange={handleChange}
                    className="col-span-3"
                />
                </div>
                <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="email" className="text-right">
                    Email
                </Label>
                <Input
                    id="email"
                    name="email"
                    type="email"
                    value={userData.user.Email}
                    onChange={handleChange}
                    className="col-span-3"
                />
                </div>
            </div>
            <DialogFooter>
                <Button type="submit">Guardar Cambios</Button>
            </DialogFooter>
            </form>
        </DialogContent>
        </Dialog>
    )
}

