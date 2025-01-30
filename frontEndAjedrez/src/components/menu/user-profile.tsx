"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { logoutAction } from "@/actions/authentication-actions";
import UserIconButton from "./user-icon-button";
import UserEditModal from "./user-edit-modal";

// Define la interfaz para los datos del usuario
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

export default function UserProfile() {
    const [userData, setUserData] = useState<UserData | null>(null);

    const [isModalOpen, setIsModalOpen] = useState(false)

    const handleOpenModal = () => setIsModalOpen(true)
    const handleCloseModal = () => setIsModalOpen(false)

    const handleSaveUserData = (newUserData: UserData) => {
        setUserData(newUserData)
        // In a real application, you would typically send this data to a server here
        console.log("Saving user data:", newUserData)
    }

    useEffect(() => {
        const userCookie = Cookies.get("userData");
        console.log("Cookie obtenida (sin decodificar):", userCookie);

        if (userCookie) {
        try {
            // Decodifica y parsea la cookie
            const decodedCookie = decodeURIComponent(userCookie);
            console.log("Cookie decodificada:", decodedCookie);

            const parsedData: UserData = JSON.parse(decodedCookie);
            console.log("Datos parseados:", parsedData);

            setUserData(parsedData);
        } catch (error) {
            console.error("Error al decodificar o parsear la cookie:", error);
        }
        } else {
        console.warn("La cookie userData no está disponible.");
        }
    }, []);

    // Si los datos del usuario no están listos, muestra un estado de carga
    if (!userData) {
        return <div>Cargando...</div>;
    }

    return (
        <div className="bg-gradient-to-r from-accent to-primary p-6 rounded-lg shadow-lg text-white">
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                <Avatar className="h-20 w-20 border-2 border-white">
                    <AvatarImage src={`https://localhost:7218/${userData.user.Avatar}`} alt={userData.user.NickName} />
                    <AvatarFallback>
                    {userData.user.NickName.slice(0, 2).toUpperCase()}
                    </AvatarFallback>
                </Avatar>
                <div>
                    <h2 className="text-2xl font-bold">{userData.user.NickName}</h2>
                    <p className="text-sm opacity-75">{userData.user.Email}</p>
                    <UserIconButton onClick={handleOpenModal} />
                    <UserEditModal
                        isOpen={isModalOpen}
                        onClose={handleCloseModal}
                        onSave={handleSaveUserData}
                        initialData={userData}
                    />
                </div>
                </div>
                <Button className="bg-foreground hover:border-accent hover:bg-background" variant="outline" onClick={() => logoutAction()}>
                Logout
                </Button>
            </div>
            <div className="mt-4 flex justify-around ">
                <div className="text-center">
                    <p className="text-2xl font-bold">ID: {userData.user.Id}</p>
                    <p className="text-sm opacity-75">Unique Identifier</p>
                </div>
                <div className="text-center">
                    <p className="text-2xl font-bold">{"24"}</p>
                    <p className="text-sm opacity-75">Wins</p>
                </div>
                <div className="text-center">
                    <p className="text-2xl font-bold">{"12"}</p>
                    <p className="text-sm opacity-75">Losses</p>
                </div>
                <div className="text-center">
                    <p className="text-2xl font-bold">{"54.0"}%</p>
                    <p className="text-sm opacity-75">Win Rate</p>
                </div>
            </div>
        </div>
    );
}
