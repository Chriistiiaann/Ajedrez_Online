"use client"

import { createContext, useContext, useState } from "react";

const UserContext = createContext({});

export function UserProvider({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {

    const [userData, setUserData] = useState({});
    const [token, setToken] = useState("");

    return (
        <UserContext.Provider value={{userData, setUserData, token, setToken}}>
            {children}
        </UserContext.Provider>
    )
}

export function useUserContext() {
    return useContext(UserContext);
}