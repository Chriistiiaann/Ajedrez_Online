"use client"

import UserProfile from '@/components/menu/user-profile'
import FriendsList from '@/components/menu/user-list'
import InboxButton from '@/components/menu/inbox-button'
import GlobalStats from '@/components/menu/global-stats'
import { PlaySection } from '@/components/menu/play-section'
 import { useWebsocketContext } from '@/contexts/webContext-Context' 
 import {useEffect, useState} from "react";

 type WebSocketMessage = {
    totalUsersConnected: number;  
  };

export default function MenuPage() {
    const [messages, setMessages] = useState<WebSocketMessage>();
   const {socket} = useWebsocketContext();
   useEffect(() =>{
    if (socket){
        const handleMessage =  (event: MessageEvent) => {
            try {
                const newMessage: WebSocketMessage = JSON.parse(event.data); 
             //   setMessages((prev) => [...prev, newMessage]); 
                setMessages(newMessage); 
              } catch (error) {
                console.error("Error al parsear mensaje:", error);
              }

        }
        socket.addEventListener("message", handleMessage);
        return () => {
            socket.removeEventListener("message", handleMessage); 
          };
    }
   }, [socket])

/*    const sendMessage = () => {
    if (socket && socket.readyState === WebSocket.OPEN) {
      const message = { totalUsersConnected: 999 }; 
      socket.send(JSON.stringify(message));
      console.log("Mensaje enviado:", message);
    } else {
      console.error("WebSocket no está conectado.");
    }
  };
  */
    return (
        <div className="flex h-screen">
            <main className="flex-1 overflow-y-auto p-4 md:p-6">

            <div>
      <h2>Mensajes probando del WebSocket</h2>
      
      <strong>{messages?.totalUsersConnected ?? "Cargando..."}</strong>

       
    </div>

            <div className="container mx-auto space-y-6">
                 <UserProfile /> 
                <div>
                    <PlaySection />
                </div>
                
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="md:col-span-2">
                  
                    <FriendsList /> 
                   
                </div>
                <div className="space-y-6">
                      <InboxButton /> 
                  
                    <GlobalStats /> 
                 
                </div>
                </div>
            </div>
            </main>
        </div>
    )
}