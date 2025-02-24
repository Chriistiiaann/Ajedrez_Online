"use client";

import React, { createContext, useContext, useState, useEffect, ReactNode } from "react";
import Cookies from "js-cookie";

// Define la interfaz para la información esencial del usuario
export interface UserContextData {
  name: string;
  email: string;
  avatar: string;
}

export interface UserData {
  token: {
    accessToken: string;
  };
  user: {
    Id: number;
    NickName: string;
    Email: string;
    Avatar: string;
    nbf: number;
    exp: number;
    iat: number;
  };
}

type UserContextType = {
  userDataContext: UserData | null;
  setUserDataContext: React.Dispatch<React.SetStateAction<UserData | null>>;
  token: string;
  setToken: React.Dispatch<React.SetStateAction<string>>;
};

const UserContext = createContext<UserContextType | undefined>(undefined);

type UserProviderProps = {
  children: ReactNode;
};

export const UserProvider = ({ children }: UserProviderProps) => {
  const [userDataContext, setUserDataContext] = useState<UserData | null>(null);
  const [token, setToken] = useState<string>("");

  useEffect(() => {
    const userCookie = Cookies.get("userData");

    if (userCookie) {
      try {
        const decodedCookie = decodeURIComponent(userCookie);
        const parsedData: UserData = JSON.parse(decodedCookie);

        console.log("Datos parseados en el UserProvider:", parsedData);
        setUserDataContext(parsedData);
      } catch (error) {
        console.error("Error al decodificar o parsear la cookie:", error);
      }
    } else {
      console.warn("La cookie userData no está disponible.");
    }
  }, []); // 🔹 Se ejecuta solo una vez cuando se monta el componente

  return (
    <UserContext.Provider value={{ userDataContext, setUserDataContext, token, setToken }}>
      {children}
    </UserContext.Provider>
  );
};

export const useUserContext = (): UserContextType => {
  const context = useContext(UserContext);
  if (!context) {
    throw new Error("useUserContext must be used within a UserProvider");
  }
  return context;
};
