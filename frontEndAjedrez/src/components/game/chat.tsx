"use client"

import type React from "react"

import { useState, useRef, useEffect } from "react"
import { Send } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"

type Message = {
    id: string
    sender: "player1" | "player2" | "system"
    content: string
    timestamp: Date
}

type ChatProps = {
    gameId: string
    currentPlayer: "player1" | "player2"
}

export default function Chat({ gameId, currentPlayer = "player1" }: ChatProps) {
    const [messages, setMessages] = useState<Message[]>([
        {
        id: "1",
        sender: "system",
        content: "Game started. Good luck!",
        timestamp: new Date(),
        },
    ])
    const [input, setInput] = useState("")
    const scrollAreaRef = useRef<HTMLDivElement>(null)

    // Auto-scroll to bottom when new messages arrive
    useEffect(() => {
        if (scrollAreaRef.current) {
        const scrollContainer = scrollAreaRef.current.querySelector("[data-radix-scroll-area-viewport]")
        if (scrollContainer) {
            scrollContainer.scrollTop = scrollContainer.scrollHeight
        }
        }
    }, [messages])

    // In a real app, you would fetch previous messages here
    useEffect(() => {
        // Simulating fetching previous messages
        console.log(`Fetching messages for game: ${gameId}`)
    }, [gameId])

    const handleSendMessage = (e: React.FormEvent) => {
        e.preventDefault()
        if (!input.trim()) return

        const newMessage: Message = {
        id: Date.now().toString(),
        sender: currentPlayer,
        content: input.trim(),
        timestamp: new Date(),
        }

        setMessages([...messages, newMessage])
        setInput("")

        // In a real app, you would send the message to a backend here
    }

    const getAvatarInfo = (sender: string) => {
        switch (sender) {
        case "player1":
            return { initials: "P1", color: "bg-primary" }
        case "player2":
            return { initials: "P2", color: "bg-secondary" }
        default:
            return { initials: "SYS", color: "bg-accent" }
        }
    }

    return (
        <div className="bg-foreground shadow rounded-lg p-4 flex flex-col h-full">
        <h2 className="text-xl font-semibold mb-2">Chat</h2>

        <ScrollArea className="flex-1 h-48 mb-4" ref={scrollAreaRef}>
            <div className="space-y-4 p-1">
            {messages.map((message) => {
                const { initials, color } = getAvatarInfo(message.sender)
                const isCurrentUser = message.sender === currentPlayer

                return (
                <div
                    key={message.id}
                    className={`flex items-start gap-2 ${isCurrentUser ? "flex-row-reverse" : "flex-row"}`}
                >
                    <Avatar className={`h-8 w-8 ${color}`}>
                    <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>

                    <div
                    className={`rounded-lg px-3 py-2 max-w-[80%] text-sm ${
                        message.sender === "system"
                        ? "bg-muted text-muted-foreground"
                        : isCurrentUser
                            ? "bg-primary text-primary-foreground"
                            : "bg-secondary text-secondary-foreground"
                    }`}
                    >
                    {message.content}
                    <div className="text-xs opacity-70 mt-1">
                        {message.timestamp.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
                    </div>
                    </div>
                </div>
                )
            })}
            </div>
        </ScrollArea>

        <form onSubmit={handleSendMessage} className="flex gap-2">
            <Input
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Type a message..."
            className="flex-1"
            />
            <Button type="submit" size="icon" className="bg-primary">
            <Send className="h-4 w-4" />
            </Button>
        </form>
        </div>
    )
}

