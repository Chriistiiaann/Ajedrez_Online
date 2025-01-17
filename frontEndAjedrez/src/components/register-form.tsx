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
          <form>
            <div className="grid gap-6">
            <div className="grid gap-2">
                <Label htmlFor="avatar">Avatar</Label>
                <Input
                  id="avatar"
                  type="file"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="apodo">Apodo</Label>
                <Input
                  id="apodo"
                  type="text"
                  placeholder="Ejemplo"
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="email">Correo Electrónico</Label>
                <Input
                  id="email"
                  type="email"
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
