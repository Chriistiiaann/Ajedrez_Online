import { Button } from "@/components/ui/button"
import { useState } from "react"
import { useWebsocketContext } from "@/contexts/webContext-Context"

type RejectButtonProps = {
  requestId: number
}

export const RejectButton = ({ requestId }: RejectButtonProps) => {
  const [loading, setLoading] = useState(false)
  const { sendMessage } = useWebsocketContext()

  const handleAccept = () => {
    setLoading(true)

    sendMessage("rejectFriendRequest", String(requestId))
    setLoading(false)
    window.location.reload()
  }

  return (
    <Button
      size="sm"
      className="mr-2 hover:bg-gray-800"
      variant="outline"
      onClick={handleAccept}
      disabled={loading}
    >
      {loading ? "Rechazando..." : "Rechazar"}
    </Button>
  )
}
