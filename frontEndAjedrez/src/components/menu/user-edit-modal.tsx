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

interface FormData {
    Id: number
    File: string
    NickName: string
    Email: string
    Password: string
}


export default function UserEditModal({ isOpen, onClose, onSave, initialData }: UserEditModalProps) {
    const [userData, setUserData] = useState<UserData>(initialData)
    const [dataToSend, setDataToSend] = useState<FormData>()
    const [passwordConfirm, setPasswordConfirm] = useState("")
    const [error, setError] = useState<string | null>(null)

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target
        setUserData((prev) => ({ ...prev, [name]: value }))
    }

    const handlePasswordConfirm = (e: React.ChangeEvent<HTMLInputElement>) => {
        setPasswordConfirm(e.target.value)
    }

    const handleImageChange = (image: string) => {
        setUserData((prev) => ({ ...prev, image }))
    }

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()
        onSave(userData)
        onClose()
    }

    const updateUserData = () => {

        if (dataToSend?.Password !== passwordConfirm) {
            setError("Las contraseñas no coinciden.")
            return
        }
        
        

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
                <Label htmlFor="password" className="text-right">
                    Contraseña
                </Label>
                <Input
                    id="password"
                    name="password"
                    type="password"
                    placeholder="Introduce tu nueva contraseña"
                    onChange={handleChange}
                    className="col-span-3"
                />
                <Label htmlFor="passwordConfirm" className="text-right">
                    Confirmar Contraseña
                </Label>
                <Input
                    id="passwordConfirm"
                    name="passwordConfirm"
                    type="password"
                    placeholder="Repite tu nueva contraseña"
                    onChange={handlePasswordConfirm}
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

