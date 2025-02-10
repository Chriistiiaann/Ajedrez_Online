"use client"

import { Button } from "@/components/ui/button"
import { UserRoundPen } from "lucide-react"

interface UserIconButtonProps {
    onClick: () => void
}

export default function UserIconButton({ onClick }: UserIconButtonProps) {
    return (
        <Button
        variant="ghost"
        size="icon"
        className="p-4 hover:bg-gray-800 rounded-full transition-colors duration-200"
        onClick={onClick}
        >
        <UserRoundPen className="h-8 w-8 text-white hover:text-gray-900 transition-colors" />
        <span className="sr-only">Edit user</span>
        </Button>
    )
}

