import Link from "next/link"

import { RegisterForm } from "@/components/register-form"

import { CastleIcon as ChessKnight } from 'lucide-react'

export default function RegisterPage() {
  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center gap-6 bg-background p-6 md:p-10 overflow-hidden">

      <div className="flex w-full max-w-sm flex-col gap-6 z-10">
        <Link href="/" className="flex items-center gap-2 self-center font-medium text-white">
          <ChessKnight className="h-6 w-6" />
          scacchi        
        </Link>
        <RegisterForm />
      </div>
    </div>
  )
}



