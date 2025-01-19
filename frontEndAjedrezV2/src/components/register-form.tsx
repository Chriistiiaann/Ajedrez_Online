import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import Link from "next/link"

import { register } from "@/actions/authentication-actions"

export function RegisterForm({
  className,
  ...props
}: React.ComponentPropsWithoutRef<"div">) {

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="bg-background shadow-sm shadow-gray-900">
        <CardHeader className="text-center">
          <CardTitle className="text-xl">¡Regístrate!</CardTitle>
        </CardHeader>
        <CardContent>
          <form action={register}>
            <div className="grid gap-6">
            <div className="grid gap-2">
                <Label htmlFor="avatar">Avatar</Label>
                <Input
                  id="avatar"
                  type="file"
                  name="avatar"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="apodo">Apodo</Label>
                <Input
                  id="apodo"
                  type="text"
                  name="nickname"
                  placeholder="Ejemplo"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="email">Correo Electrónico</Label>
                <Input
                  id="email"
                  type="email"
                  name="email"
                  placeholder="Ejemplo@gmail.com"
                  required
                />
              </div>
              <div className="grid gap-2">
                <div className="flex items-center">
                  <Label htmlFor="password">Contaseña</Label>
                </div>
                <Input
                  id="password"
                  type="password"
                  name="password"
                  placeholder="Contraseña"
                  required />
              </div>
              <div className="grid gap-2">
                <div className="flex items-center">
                  <Label htmlFor="password-Check">Confirmar Contraseña</Label>
                </div>
                <Input
                  id="password-Check"
                  type="password"
                  name="password-Check"
                  placeholder="Contraseña"
                  required />
              </div>
              <Button type="submit" className="w-full">
                Regístrarse
              </Button>
            </div>
            <div className="text-center text-sm mt-4">
              ¿Tienes una cuenta? 
              <Link href="/login" className="underline underline-offset-4">
                Iniciar Sesión
              </Link>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
