import { obtener } from "@/services/ajax_service";

import type { ApiResponse } from "@/models/api_response";
import type { ResultadoPaginado } from "@/models/paginacion";
import type { Usuario } from "@/models/usuario";

interface UsuarioApi {
  id: number;
  nombre: string;
  correo: string;
  telefono?: string | null;
  dni: string;
  rol: string | null;
  estado: string;
  avatar?: string | null;
  direccion?: string | null;
}

export async function obtener_usuarios(
  pagina = 1,
  limit = 10,
  termino?: string,
  sort?: string,
): Promise<ResultadoPaginado<Usuario>> {
  const parametros = new URLSearchParams({
    page: String(pagina),
    limit: String(limit),
  });

  if (termino) {
    parametros.set("termino", termino);
  }

  if (sort) {
    parametros.set("sort", sort);
  }

  const respuesta = await obtener<ApiResponse<ResultadoPaginado<UsuarioApi>>>(
    `usuarios?${parametros.toString()}`,
  );

  if (!respuesta.exito || !respuesta.datos) {
    throw new Error(respuesta.mensaje || "No se pudieron cargar los usuarios.");
  }

  return {
    datos: respuesta.datos.datos.map((usuario) => ({
      id: String(usuario.id),
      nombre: usuario.nombre,
      correo: usuario.correo,
      telefono: usuario.telefono ?? undefined,
      dni: usuario.dni,
      rol: usuario.rol as Usuario["rol"],
      estado: usuario.estado as Usuario["estado"],
      avatar: usuario.avatar ?? null,
      direccion: usuario.direccion ?? undefined,
    })),
    page: respuesta.datos.page,
    limit: respuesta.datos.limit,
    offset: respuesta.datos.offset,
    total: respuesta.datos.total,
    totalPaginas: respuesta.datos.totalPaginas,
  };
}
