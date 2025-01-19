//Deshabilitar certificado!!!!!!!!!!!!!!!!!!!!!!!!
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'

import { jwtDecode } from "jwt-decode";

export const login = async (formData: FormData) => {
    "use server"

    // Obtener los valores de usuario y contraseña
    const user = formData.get("identifier") as string;
    const password = formData.get("password") as string;

    // Verificar si los valores están vacíos
    if (!user || !password) {
        console.error("Missing username or password");
        return;
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
            return;
        }

        // Verificar que la respuesta sea JSON
        const contentType = response.headers.get("Content-Type");
        if (contentType && contentType.includes("application/json")) {
            // Leer directamente el JSON desde la respuesta
            const data = await response.json();
            const decodedToken = jwtDecode(data.accessToken);
            console.log("User data:", decodedToken); // Aquí deberías recibir el token o la respuesta esperada
            console.log("Token:", data);
        } else {
            console.error("Expected JSON, but received:", contentType);
        }

    } catch (err) {
        console.error("Error during login:", err);
    }
};


export const register = async (formData: FormData) => {
    "use server"

    const avatar = formData.get("avatar") as File;
    const nickname = formData.get("nickname") as string;
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    console.log("datos a enviar", { avatar, nickname, email, password });

    if (!nickname || !email || !password || !avatar) {
        console.error("Missing required fields");
        return;
    }

    const registerFormData = new FormData();
    registerFormData.append("File", avatar);  // Agregar archivo
    registerFormData.append("NickaNme", nickname);
    registerFormData.append("Email", email);
    registerFormData.append("Password", password);

    try {
        const response = await fetch("https://localhost:7218/api/User/register", {
            method: "POST",
            body: registerFormData,  // Enviar FormData con archivo
        });

        if (response.ok) {
            const data = await response.json();
            console.log("Registration successful:", data);
        } else {
            const errorText = await response.text();
            console.error("Registration failed with error:", errorText);
        }
    } catch (err) {
        console.error("Error during registration:", err);
    }
};

