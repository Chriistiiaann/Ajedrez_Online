import { cookies } from "next/headers";
export const cookieLogin = async (userData: object) => {
    const cookieStore = await cookies();
    cookieStore.set({
        name: "userData",
        value: JSON.stringify(userData),
        httpOnly: false,
        secure: process.env.NODE_ENV === "production",
        path: "/",
        maxAge: 60 * 60 * 24 * 7,
    });
};
