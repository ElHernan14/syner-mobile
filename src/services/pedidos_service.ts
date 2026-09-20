import { obtener } from "@/services/ajax_service";

import type { ApiResponse } from "@/models/api_response";
import type { ResultadoPaginado } from "@/models/paginacion";
import type { Pedido } from "@/models/pedido";

interface PedidoApi {
  id: number;
  estado: string;
  numero_seguimiento?: string | null;
  codigo_entrega?: string | null;
  usuario: {
    id: number;
    nombre: string;
    correo: string;
  };
  lote: {
    id: number;
    nombre: string;
    categoria: string;
    precio_cupo: number;
  };
}

export async function obtener_pedidos(
  pagina = 1,
  limit = 10,
  termino?: string,
  sort?: string,
): Promise<ResultadoPaginado<Pedido>> {
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

  const respuesta = await obtener<ApiResponse<ResultadoPaginado<PedidoApi>>>(
    `pedidos?${parametros.toString()}`,
  );

  if (!respuesta.exito || !respuesta.datos) {
    throw new Error(respuesta.mensaje || "No se pudieron cargar los pedidos.");
  }

  return {
    datos: respuesta.datos.datos.map((pedido) => ({
      id: String(pedido.id),
      estado: pedido.estado,
      numeroSeguimiento: pedido.numero_seguimiento ?? undefined,
      codigoEntrega: pedido.codigo_entrega ?? undefined,
      usuario: {
        id: String(pedido.usuario.id),
        nombre: pedido.usuario.nombre,
        correo: pedido.usuario.correo,
      },
      lote: {
        id: String(pedido.lote.id),
        nombre: pedido.lote.nombre,
        categoria: pedido.lote.categoria,
        precioCupo: pedido.lote.precio_cupo,
      },
    })),
    page: respuesta.datos.page,
    limit: respuesta.datos.limit,
    offset: respuesta.datos.offset,
    total: respuesta.datos.total,
    totalPaginas: respuesta.datos.totalPaginas,
  };
}
