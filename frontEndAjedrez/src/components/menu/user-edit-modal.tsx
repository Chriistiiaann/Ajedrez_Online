"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import ImageUpload from "@/components/menu/image-upload";
import type { UserData } from "@/components/menu/user-profile";

interface UserEditModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: (userData: UserData) => void;
    initialData: UserData;
}

interface FormData {
    Id: number;
    NickName: string;
    Email: string;
    Password: string;
    File: string;
}

export default function UserEditModal({ isOpen, onClose, onSave, initialData }: UserEditModalProps) {
    const [userData, setUserData] = useState<UserData>(initialData);
    const [passwordConfirm, setPasswordConfirm] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [formData, setFormData] = useState<FormData>({
        Id: initialData.user.Id,
        NickName: initialData.user.NickName,
        Email: initialData.user.Email,
        Password: "",
        File: initialData.user.Avatar || "",
    });

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        if (name === "NickName" || name === "Email" || name === "Password") {
            setFormData((prev) => ({ ...prev, [name]: value }));
        }
    };

    const handlePasswordConfirm = (e: React.ChangeEvent<HTMLInputElement>) => {
        setPasswordConfirm(e.target.value);
    };

    const handleImageChange = (image: string) => {
        setFormData((prev) => ({ ...prev, File: image }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (formData.Password && formData.Password !== passwordConfirm) {
            setError("Las contraseñas no coinciden.");
            return;
        }

        setError(null);

        try {
            console.log("Datos enviados al servidor:", formData); // Depuración

            const response = await fetch("https://localhost:7218/api/User/update", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${initialData.token.accessToken}`,
                },
                body: JSON.stringify({
                    Id: formData.Id,
                    NickName: formData.NickName,
                    Email: formData.Email,
                    Password: formData.Password || "",
                    File: formData.File || "",
                }),
            });

            if (!response.ok) {
                const errorData = await response.text(); // Capturar respuesta cruda
                console.error("Respuesta del servidor:", errorData); // Depuración
                throw new Error(`Error ${response.status}: ${errorData || "Error al actualizar los datos del usuario"}`);
            }

            const updatedUserData: UserData = {
                ...userData,
                user: {
                    ...userData.user,
                    NickName: formData.NickName,
                    Email: formData.Email,
                    Avatar: formData.File,
                },
            };

            onSave(updatedUserData);
            onClose();
        } catch (err: any) {
            setError(err.message || "Error al guardar los cambios");
            console.error("Error completo:", err); // Depuración
        }
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="text-white sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>Editar Perfil</DialogTitle>
                </DialogHeader>
                <form onSubmit={handleSubmit}>
                    <div className="grid gap-4 py-4">
                        <ImageUpload
                            currentImage={`https://localhost:7218/${userData.user.Avatar}`}
                            onImageChange={handleImageChange}
                        />
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="NickName" className="text-right">
                                Nombre
                            </Label>
                            <Input
                                id="NickName"
                                name="NickName"
                                value={formData.NickName}
                                onChange={handleChange}
                                className="col-span-3"
                            />
                        </div>
                        <div className="grid grid-cols-4 items-center gap-4">
                            <Label htmlFor="Email" className="text-right">
                                Email
                            </Label>
                            <Input
                                id="Email"
                                name="Email"
                                type="email"
                                value={formData.Email}
                                onChange={handleChange}
                                className="col-span-3"
                            />
                            <Label htmlFor="Password" className="text-right">
                                Contraseña
                            </Label>
                            <Input
                                id="Password"
                                name="Password"
                                type="password"
                                placeholder="Introduce tu nueva contraseña"
                                value={formData.Password}
                                onChange={handleChange}
                                className="col-span-3"
                            />
                            <Label htmlFor="passwordConfirm" className="text-right">
                                Confirmar Contraseña
                            </Label>
                            <Input
                                id="passwordConfirm"
                                name="passwordConfirm"
                                type="password"
                                placeholder="Repite tu nueva contraseña"
                                value={passwordConfirm}
                                onChange={handlePasswordConfirm}
                                className="col-span-3"
                            />
                        </div>
                        {error && <p className="text-red-500 text-sm">{error}</p>}
                    </div>
                    <DialogFooter>
                        <Button type="submit">Guardar Cambios</Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}