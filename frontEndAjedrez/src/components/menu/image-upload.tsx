"use client"

import { useState, useRef } from "react"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Camera } from "lucide-react"

interface ImageUploadProps {
    currentImage: string
    onImageChange: (image: string) => void
}

export default function ImageUpload({ currentImage, onImageChange }: ImageUploadProps) {
    const [previewImage, setPreviewImage] = useState(currentImage)
    const fileInputRef = useRef<HTMLInputElement>(null)

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0]
        if (file) {
        const reader = new FileReader()
        reader.onloadend = () => {
            const imageDataUrl = reader.result as string
            setPreviewImage(imageDataUrl)
            onImageChange(imageDataUrl)
        }
        reader.readAsDataURL(file)
        }
    }

    return (
        <div className="flex flex-col items-center space-y-4">
        <Avatar className="w-24 h-24">
            <AvatarImage src={previewImage} alt="User avatar" />
            <AvatarFallback>{currentImage ? currentImage.slice(0, 2).toUpperCase() : "US"}</AvatarFallback>
        </Avatar>
        <Button variant="outline" size="sm" onClick={() => fileInputRef.current?.click()}>
            <Camera className="mr-2 h-4 w-4" />
            Cambiar Avatar
        </Button>
        <input type="file" ref={fileInputRef} onChange={handleImageChange} accept="image/*" className="hidden" />
        </div>
    )
}

