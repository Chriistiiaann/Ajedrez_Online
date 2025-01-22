"use client"

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
import { useActionState } from "react"

import { registerAction } from "@/actions/authentication-actions"

export function RegisterForm({
  className,
  ...props
}: React.ComponentPropsWithoutRef<"div">) {

  const [actionState, action] = useActionState(registerAction, {
      message: "",
      fieldErrors: {},
    });

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <Card className="bg-background shadow-sm shadow-gray-900">
        <CardHeader className="text-center">
          <CardTitle className="text-xl">¡Regístrate!</CardTitle>
        </CardHeader>
        <CardContent>
          <form action={action}>
            <div className="grid gap-6">
            <div className="grid gap-2">
                <Label htmlFor="avatar">Avatar</Label>
                <Input
                  id="avatar"
                  type="file"
                  name="avatar"
                  required
                />
                {actionState?.fieldErrors?.avatar && (
                  <p className="px-1 text-xs text-red-600">
                    {actionState.fieldErrors.avatar}
                  </p>
                )}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="apodo">Apodo</Label>
                <Input
                  id="apodo"
                  type="text"
                  name="nickname"
                  placeholder="Ejemplo"
                  defaultValue={actionState?.payload?.get("nickname") as string}
                  required
                />
                {actionState?.fieldErrors?.nickname && (
                  <p className="px-1 text-xs text-red-600">
                    {actionState.fieldErrors.nickname}
                  </p>
                )}
              </div>
              <div className="grid gap-2">
                <Label htmlFor="email">Correo Electrónico</Label>
                <Input
                  id="email"
                  type="email"
                  name="email"
                  placeholder="Ejemplo@gmail.com"
                  defaultValue={actionState?.payload?.get("email") as string}
                  required
                />
                {actionState?.fieldErrors?.email && (
                  <p className="px-1 text-xs text-red-600">
                    {actionState.fieldErrors.email}
                  </p>
                )}
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
                {actionState?.fieldErrors?.password && (
                  <p className="px-1 text-xs text-red-600">
                    {actionState.fieldErrors.password}
                  </p>
                )}
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
                {actionState?.fieldErrors?.passwordCheck && (
                  <p className="px-1 text-xs text-red-600">
                    {actionState.fieldErrors.passwordCheck}
                  </p>
                )}
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
