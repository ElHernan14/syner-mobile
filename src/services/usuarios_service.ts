import { obtener } from "@/services/ajax_service";

import type { Usuario } from "@/data/usuario";

interface UsuarioApi {
  id: number;
  nombre: string;
  correo: string;
  telefono?: string | null;
  dni: string;
  rol: string;
  estado: string;
  avatar?: string | null;
  direccion?: string | null;
}

export async function obtener_usuarios(): Promise<Usuario[]> {
  const respuesta = await obtener<UsuarioApi[]>("usuarios");

  return respuesta.map((usuario) => ({
    id: String(usuario.id),
    nombre: usuario.nombre,
    correo: usuario.correo,
    telefono: usuario.telefono ?? undefined,
    dni: usuario.dni,
    rol: usuario.rol as Usuario["rol"],
    estado: usuario.estado as Usuario["estado"],
    avatar: usuario.avatar ?? null,
    direccion: usuario.direccion ?? undefined,
  }));
}
