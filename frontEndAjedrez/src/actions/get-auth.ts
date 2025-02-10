"use server"
import { cookies } from "next/headers";
import { cache } from "react";
import { jwtDecode, } from "jwt-decode";

type decodedToken = {
  Id: number,
  Nickname: string,
  Avatar: string,
  Email: string,
  Exp: number,
}

export const getAuth = cache(async () => {
  const cookieStore = await cookies();

  const authToken = cookieStore.get("userData")?.value ?? null;
  const decodedToken = authToken ? jwtDecode<decodedToken>(authToken) : null;

  if (!authToken) {
    return {
      token:null,
      decodedToken:null
    };
  }

  return {
    token: authToken,
    decodedToken: decodedToken
  };
});