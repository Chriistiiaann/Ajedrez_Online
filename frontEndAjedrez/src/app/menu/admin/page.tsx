"use client"

import { useState, useEffect, useCallback } from "react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { ArrowLeft, Loader2 } from "lucide-react"
import { Badge } from "@/components/ui/badge"

type User = {
  id: string
  name: string
  email: string
  role: string
}

type RoleOption = "None" | "Admin" | "Banned"

export default function AdminPage() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [selectedUser, setSelectedUser] = useState<User | null>(null)
  const [updatingRole, setUpdatingRole] = useState(false)

  const roleOptions: RoleOption[] = ["None", "Admin", "Banned"]

  const fetchUsers = useCallback(async () => {
    setLoading(true)
    try {
      const response = await fetch("https://localhost:7218/api/User")
      if (!response.ok) {
        throw new Error("Failed to fetch users")
      }
      const data = await response.json()
      setUsers(data)
      setError(null)
    } catch (err) {
      setError("Error loading users. Please try again.")
      console.error(err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchUsers()
  }, [fetchUsers])

  const handleRoleClick = (user: User) => {
    setSelectedUser(user)
    setIsModalOpen(true)
  }

  const handleRoleChange = async (newRole: RoleOption) => {
    if (!selectedUser) return

    setUpdatingRole(true)
    try {
      const response = await fetch("https://localhost:7218/api/User/update-role", {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          userId: selectedUser.id,
          newRole: newRole,
        }),
      })

      if (!response.ok) {
        throw new Error("Failed to update role")
      }

      // Close modal and refresh users
      setIsModalOpen(false)
      await fetchUsers()
    } catch (err) {
      console.error(err)
      setError("Error updating role. Please try again.")
    } finally {
      setUpdatingRole(false)
    }
  }

  const getRoleBadgeVariant = (role: string) => {
    switch (role.toLowerCase()) {
      case "admin":
        return "destructive"
      case "banned":
        return "outline"
      default:
        return "secondary"
    }
  }

  return (
    <div className="container mx-auto py-8 text-white">
      <div className="flex items-center mb-6">
        <Link href="/">
          <Button variant="outline" size="sm" className="mr-4">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Volver al menu
          </Button>
        </Link>
        <h1 className="text-3xl font-bold">Admin Dashboard</h1>
      </div>

      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle>Administrar Usuarios</CardTitle>
            <Button onClick={fetchUsers} disabled={loading}>
              {loading ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : "Refrescar"}
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {error && <p className="text-red-500 mb-4">{error}</p>}

          {loading ? (
            <div className="flex justify-center items-center py-8">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b">
                    <th className="text-left py-3 px-4">ID</th>
                    <th className="text-left py-3 px-4">Nombre</th>
                    <th className="text-left py-3 px-4">Email</th>
                    <th className="text-left py-3 px-4">Role</th>
                  </tr>
                </thead>
                <tbody>
                  {users.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-center py-4">
                        No se encontraron usuarios
                      </td>
                    </tr>
                  ) : (
                    users.map((user) => (
                      <tr key={user.id} className="border-b hover:bg-muted/50">
                        <td className="py-3 px-4">{user.id}</td>
                        <td className="py-3 px-4">{user.name}</td>
                        <td className="py-3 px-4">{user.email}</td>
                        <td className="py-3 px-4">
                          <Badge
                            variant={getRoleBadgeVariant(user.role)}
                            className="cursor-pointer"
                            onClick={() => handleRoleClick(user)}
                          >
                            {user.role}
                          </Badge>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Role Change Modal */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent className="text-white">
          <DialogHeader>
            <DialogTitle>Quieres cambiar el rol del usuario?</DialogTitle>
            <DialogDescription></DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            {roleOptions.map((role) => (
              <Button
                key={role}
                variant={selectedUser?.role === role ? "default" : "outline"}
                onClick={() => handleRoleChange(role)}
                disabled={updatingRole}
              >
                {updatingRole ? <Loader2 className="h-4 w-4 animate-spin mr-2" /> : null}
                {role}
              </Button>
            ))}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              Cancelar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}

