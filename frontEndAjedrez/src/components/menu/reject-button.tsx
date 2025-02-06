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
  }

  return (
    <Button
      size="sm"
      className="mr-2"
      variant="outline"
      onClick={handleAccept}
      disabled={loading}
    >
      {loading ? "Rechazando..." : "Rechazar"}
    </Button>
  )
}
