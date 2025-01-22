import Link from "next/link"

import { LoginForm } from "@/components/login-form"
import { CastleIcon as ChessKnight } from 'lucide-react'

import { cookies } from "next/headers";
import { redirect } from "next/navigation";

export default async function LoginPage() {

  const cookieStore = await cookies();
    
  const userData = cookieStore.get("userData")?.value ?? null
  
  if (!userData) {

    return (
      <div className="relative flex min-h-svh flex-col items-center justify-center gap-6 bg-background p-6 md:p-10 overflow-hidden">
        

        <div className="flex w-full max-w-sm flex-col gap-6 z-10 ">
          <Link href="/" className="flex items-center gap-2 self-center font-medium text-white">
            <ChessKnight className="h-6 w-6" />
            scacchi        
          </Link>
          <LoginForm />
        </div>
      </div>
    )
  } else {
    redirect("/menu")
  }
}



