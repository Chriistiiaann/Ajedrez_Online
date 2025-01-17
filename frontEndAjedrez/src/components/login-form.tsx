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

export function LoginForm({
  className,
  ...props
}: React.ComponentPropsWithoutRef<"div">) {

  // const login = async (formData: FormData) => {
  //   "use server"

  //   const email = formData.get("email")
  //   const password = formData.get("password")

  //   console.log(email, password)

  // }

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="bg-background shadow-sm shadow-gray-900">
        <CardHeader className="text-center">
          <CardTitle className="text-xl">¡Bienvenido de nuevo!</CardTitle>
        </CardHeader>
        <CardContent>
          <form>
            <div className="grid gap-6">
              <div className="grid gap-2">
                <Label htmlFor="email">Apodo o Correo Electrónico</Label>
                <Input
                  id="email"
                  type="text"
                  placeholder="Ejemplo || Ejemplo@gmail.com"
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
                  placeholder="Ejemplo"
                  required />
              </div>
              <Button type="submit" className="w-full bg-primary">
                Iniciar Sesión
              </Button>
            </div>
            <div className="text-center text-sm mt-4">
              ¿No tienes una cuenta? 
              <Link href="/register" className="underline underline-offset-4">
                Regístrarse
              </Link>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
