"use server";

//Deshabilitar certificado!!!!!!!!!!!!!!!!!!!!!!!!
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'

import { jwtDecode } from "jwt-decode";
import { cookies } from "next/headers";
import { cookieLogin } from "@/app/(login)/cookie-login";
import { cookieLogout } from "@/app/(login)/cokie-logout";
import { redirect } from "next/navigation";

// Definición del tipo para el estado de la acción
export type ActionState = {
    status?: "PENDING" | "SUCCESS" | "ERROR"; // Enumera los posibles estados
    message: string; // Mensaje opcional para mostrar al usuario
    payload?: FormData; // Datos asociados con la acción (como el FormData enviado)
    fieldErrors?: Record<string, string>; // Errores específicos de campos, como { email: "Invalid email" }
}


export const loginAction = async (_actionState: ActionState, formData: FormData, userFromRegister?: string, passwordFromRegister?: string): Promise<ActionState> => {

    // Obtener los valores de usuario y contraseña
    const user = userFromRegister || (formData.get("identifier") as string);
    const password = passwordFromRegister || (formData.get("password") as string);

    // Verificar si los valores están vacíos
    if (!user || !password) {
        console.error("Missing username or password");
        return { status: "ERROR", message: "Missing username or password" };
    }

    console.log({ user, password });

    // Preparar los datos del usuario para la autenticación

    const userData = {
        user,
        password
    };

    try {
        // Hacer la solicitud al servidor
        const response = await fetch("https://localhost:7218/api/Auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
            },
            body: JSON.stringify(userData),
        });

        // Verificar si la respuesta es exitosa
        if (!response.ok) {
            // Mostrar el cuerpo de la respuesta en caso de error
            const errorText = await response.text();
            console.error("Login failed with error:", errorText);
            return { status: "ERROR", message: errorText };
        }

        // Verificar que la respuesta sea JSON
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
            // Leer directamente el JSON desde la respuesta
            const data = await response.json();
            const decodedToken = jwtDecode(data.accessToken);
            console.log("User data:", decodedToken); // Aquí deberías recibir el token o la respuesta esperada
            console.log("Token:", data);

            const userData = {
                token: data,
                user: decodedToken}

            await cookieLogin(userData);
            
            const cookieStore = await cookies();

            console.log(cookieStore.get("userData")?.value ?? null)

            

        } else {
            console.error("Expected JSON, but received:", contentType);
            return { status: "ERROR", message: "Expected JSON, but received: " + contentType };
        }



    } catch (err) {
        console.error("Error during login:", err);
        return { status: "ERROR", message: "Error during login" };
    }

    redirect("/menu")
};


export const registerAction = async (_actionState: ActionState, formData: FormData): Promise<ActionState> => {
    const avatar = formData.get("avatar") as File;
    const nickname = formData.get("nickname") as string;
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;
    const passwordCheck = formData.get("password-Check") as string;

    if (!nickname || !email || !password || !avatar) {
        console.error("Missing required fields");
        return { status: "ERROR", message: "Campos requeridos no proporcionados" };
    }

    if (password !== passwordCheck) {
        console.error("Passwords do not match");
        return { status: "ERROR", message: "Las contraseñas no coinciden" };
    }

    const registerFormData = new FormData();
    registerFormData.append("Id", "0");
    registerFormData.append("File", avatar); // Archivo
    registerFormData.append("NickName", nickname);
    registerFormData.append("Email", email);
    registerFormData.append("Password", password);

    try {
        const response = await fetch("https://localhost:7218/api/User/register", {
            method: "POST",
            body: registerFormData, // Enviar como FormData
        });

        console.log("Datos a enviar", registerFormData);

        if (response.ok) {
            const data = await response.json();
            console.log("Registration successful:", data);

            
        } else {
            const errorText = await response.text();
            console.error("Registration failed with error:", errorText);
            return { status: "ERROR", message: errorText };
        }
    } catch (err) {
        console.error("Error during registration:", err);
        return { status: "ERROR", message: "Error durante el registro" };
    }
    const loginState = await loginAction(_actionState, new FormData(), email, password);

            if (loginState.status === "SUCCESS") {
                console.log("User logged in successfully after registration");
                return { status: "SUCCESS", message: "Registro e inicio de sesión exitosos" };
            } else {
                return {
                    status: "ERROR",
                    message: `Registro exitoso, pero inicio de sesión fallido: ${loginState.message}`,
                };
            }
};


export const logoutAction = async () => {
    await cookieLogout();
    redirect("/login");
}

