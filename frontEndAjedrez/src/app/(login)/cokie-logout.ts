import { cookies } from "next/headers";
export const cookieLogout = async () => {
    const cookieStore = await cookies();
    cookieStore.delete("userData");
};
