"use client";
import { usePathname } from "next/navigation";
import { createContext, useState, useContext, ReactNode, useEffect } from "react";
import { getAuth } from "@/actions/get-auth";




interface WebsocketContextType {
   socket: WebSocket | null;
   messages: Record<string, any>; 
  
}

export const WebsocketContext = createContext<WebsocketContextType | undefined>(undefined);

export const useWebsocketContext = (): WebsocketContextType => {
    const context = useContext(WebsocketContext);
    if (!context) {
        throw new Error("useWebsocketContext debe usarse dentro de un WebsocketProvider");
    }
    return context;
};

interface WebsocketProviderProps {
    children: ReactNode;
    
}



export const WebsocketProvider = ({ children }: WebsocketProviderProps) => {
    const [socket, setSocket] = useState<WebSocket | null>(null);
    const [IdToken, setIdToken] = useState<number | null>(null);
    const [messages, setMessages] = useState<Record<string, any>>({});

   
    const pathname = usePathname();
    useEffect(() => {
        async function LeerToken() {
            const authData = await getAuth();
            const idToken = authData?.decodedToken?.Id ?? null;
            setIdToken(idToken);
        }
        LeerToken();
    }, [pathname]);


    useEffect(() => {

        if (!IdToken){
            if (socket){
                socket.close(); setSocket(null); console.log("cerrando socket");
            }
            return;
        }

        if(socket) return;
       
    
        try{
            const ws = new WebSocket(`wss://localhost:7218/api/handler?userId=${IdToken}`);
        
        

        ws.onopen = () => {
            console.log("WebSocket conectado.");
            setSocket(ws);
           
        };

        ws.onmessage = (event: MessageEvent) => {
            try {
                const newMessage: Record<string, any> = JSON.parse(event.data);
                console.log("Mensaje recibido:", newMessage);

             
                if (newMessage.totalUsersConnected) {
                    setMessages((prevMessages) => ({
                        ...prevMessages,
                        ...newMessage, 
                    }));
                }
            } catch (error) {
                console.error("Error al parsear mensaje:", error);
            }
        };

        ws.onclose = () => {
            console.log("WebSocket desconectado.");
            setSocket(null);
          
        };

        ws.onerror = (error) => {
            console.error("Error en WebSocket:", error);
        }; 
        return () => {
            ws.close();
        };
    }
    catch(error){
        console.log("error websocket", error);
    }

       
    }, [IdToken]);

    const sendMessage = (message: object) => {
        if (socket && socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify(message));
        } else {
            console.warn("No hay conexión WebSocket activa.");
        }
    };

    const contextValue: WebsocketContextType = {
        socket,
        messages,
    };

    return (
        <WebsocketContext.Provider value={contextValue}>
            {children}
        </WebsocketContext.Provider>
    );
};
