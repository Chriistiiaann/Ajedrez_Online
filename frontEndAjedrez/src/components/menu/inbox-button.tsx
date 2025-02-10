'use client'

import { useEffect, useState } from 'react'
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Inbox } from 'lucide-react'
import { getAuth } from '@/actions/get-auth'
import { AcceptButton } from './accept-button'
import { RejectButton } from './reject-button'

type FriendRequest = {
    requestId: number
    nickName: string
    avatar: string
}

export default function InboxButton() {
    const [friendRequests, setFriendRequests] = useState<FriendRequest[]>([])
    const [message, setMessage] = useState<string | null>(null)
    const [loading, setLoading] = useState<boolean>(false)
    const [error, setError] = useState<string | null>(null)
    const [isOpen, setIsOpen] = useState(false)

    // Función para obtener las solicitudes de amistad del servidor
    const fetchFriendRequests = async () => {
        setLoading(true)
        setError(null)
        setMessage(null)

        try {
            // Obtener datos de autenticación (userId)
            const authData = await getAuth()
            const userId = authData.decodedToken?.Id
            if (!userId) {
                throw new Error('User ID no disponible')
            }

            const response = await fetch(`https://localhost:7218/api/Friend/pending/${userId}`)
            if (!response.ok) {
                if (response.status === 400) {
                    const errorData = await response.json()
                    setMessage(errorData.message || 'Hubo un problema al obtener las solicitudes.')
                } else {
                    setError('No se pudieron obtener las solicitudes de amistad.')
                }
                setFriendRequests([]) // Limpiamos las solicitudes si no hay datos
                return
            }

            const data = await response.json()
            if (data.message) {
                setMessage(data.message)
            } else {
                setFriendRequests(data.pendingFriendshipRequest || [])
            }
        } catch (err: any) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    // Llamar a la función cuando se monta el componente
    useEffect(() => {
        fetchFriendRequests()
    }, [])

    return (
        <Dialog open={isOpen} onOpenChange={setIsOpen}>
            <DialogTrigger asChild>
                <Button variant="outline" className="w-full bg-foreground">
                    <Inbox className="mr-2 h-4 w-4" />
                    Bandeja de entrada ({friendRequests.length})
                </Button>
            </DialogTrigger>

            <DialogContent className="text-white">
                <DialogHeader>
                    <DialogTitle>Solicitudes de amistad</DialogTitle>
                </DialogHeader>

                {loading && <p>Cargando solicitudes...</p>}
                {error && <p className="text-red-500">{error}</p>}
                {message && !loading && !error && <p className="text-red-500">{message}</p>}

                <ul className="space-y-2">
                    {friendRequests.map((request) => (
                        <li key={request.requestId} className="flex items-center justify-between p-2 rounded-md">
                            <div className="flex items-center space-x-2">
                                <img src={`https://localhost:7218/` + request.avatar} alt={request.nickName} className="w-8 h-8 rounded-full" />
                                <span>{request.nickName}</span> 
                                <span className='text-sm text-gray-400'>Id de solicitud: {request.requestId}</span>
                            </div>
                            <div>
                                <AcceptButton requestId={request.requestId} />
                                <RejectButton requestId={request.requestId} />
                            </div>
                        </li>
                    ))}
                </ul>
            </DialogContent>
        </Dialog>
    )
}
