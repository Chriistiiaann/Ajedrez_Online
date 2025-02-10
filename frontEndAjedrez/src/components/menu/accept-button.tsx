import { Button } from "@/components/ui/button"
import { useState } from "react"
import { useWebsocketContext } from "@/contexts/webContext-Context"

type AcceptButtonProps = {
  requestId: number
}

export const AcceptButton = ({ requestId }: AcceptButtonProps) => {
  const [loading, setLoading] = useState(false)
  const { sendMessage } = useWebsocketContext()

  const handleAccept = () => {
    setLoading(true)

    sendMessage("acceptFriendRequest", String(requestId))

    window.location.reload()
  }

  return (
    <Button
      size="sm"
      className="mr-2"
      variant="outline"
      onClick={handleAccept}
      disabled={loading}
    >
      {loading ? "Aceptando..." : "Aceptar"}
    </Button>
  )
}
