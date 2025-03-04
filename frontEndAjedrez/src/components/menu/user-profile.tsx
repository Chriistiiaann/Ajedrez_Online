"use client";

import { useEffect, useState } from "react";
import Cookies from "js-cookie";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { logoutAction } from "@/actions/authentication-actions";
import UserIconButton from "./user-icon-button";
import UserEditModal from "./user-edit-modal";
import { useUserContext } from "@/contexts/user-context";
import Link from "next/link";

// Define la interfaz para los datos del usuario
export interface UserData {
  token: {
    accessToken: string;
  };
  user: {
    Id: number;
    NickName: string;
    Email: string;
    Role: string;
    Avatar: string;
    nbf: number;
    exp: number;
    iat: number;
    Rol?: string; // Añadimos Rol como opcional, ajusta según tu contexto
  };
}

export default function UserProfile() {
  const [userData, setUserData] = useState<UserData | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const { userDataContext } = useUserContext();

  const handleOpenModal = () => setIsModalOpen(true);
  const handleCloseModal = () => setIsModalOpen(false);

  const handleSaveUserData = (newUserData: UserData) => {
    setUserData(newUserData);
    Cookies.set("userData", JSON.stringify(newUserData), { expires: 7 });
    console.log("User data saved and cookie updated:", newUserData);
  };

  useEffect(() => {
    const userCookie = Cookies.get("userData");
    console.log("Cookie obtenida (sin decodificar):", userCookie);

    if (userCookie) {
      try {
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

  if (!userData) {
    return <div>Cargando...</div>;
  }

  return (
    <div className="bg-gradient-to-r from-accent to-primary p-6 rounded-lg shadow-lg text-white">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <Avatar className="h-20 w-20 border-2 border-white">
            <AvatarImage
              src={`https://localhost:7218/${userData.user.Avatar}`}
              alt={userData.user.NickName}
            />
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
        <div className="flex flex-col items-center space-y-2">
          <Button
            className="bg-foreground hover:border-accent hover:bg-destructive"
            variant="outline"
            onClick={() => logoutAction()}
          >
            Cerrar sesión
          </Button>
          {userData.user.Role === "Admin" && (
            <Link href="/menu/admin">
              <Button
                className="bg-foreground hover:border-accent hover:bg-accent"
                variant="outline"
              >
                Administrador
              </Button>
            </Link>
          )}
        </div>
      </div>
      <div className="mt-4 flex justify-around">
        <div className="text-center">
          <p className="text-2xl font-bold">ID: {userData.user.Id}</p>
          <p className="text-sm opacity-75">Identificador de usuario</p>
        </div>
      </div>
    </div>
  );
}