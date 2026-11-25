import { obtener, obtener_blob, crear_form_data } from "@/services/ajax_service";

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
    precio_mercado: number;
    porcentaje_ahorro: number;
    proveedor: string;
    proveedor_verificado: boolean;
  };
}

export async function obtener_pedidos(
  pagina = 1,
  limit = 10,
  termino?: string,
  sort?: string,
  pedido_id?: number,
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

  if (pedido_id !== undefined) {
    parametros.set("pedidoId", String(pedido_id));
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
        precioMercado: pedido.lote.precio_mercado,
        porcentajeAhorro: pedido.lote.porcentaje_ahorro,
        proveedor: pedido.lote.proveedor,
        proveedorVerificado: pedido.lote.proveedor_verificado,
      },
    })),
    page: respuesta.datos.page,
    limit: respuesta.datos.limit,
    offset: respuesta.datos.offset,
    total: respuesta.datos.total,
    totalPaginas: respuesta.datos.totalPaginas,
  };
}

interface CrearPedidoCupoData {
  id: number;
  usuarioId: number;
  loteId: number;
  estado: string;
  numeroSeguimiento: string | null;
  codigoEntrega: string | null;
}

interface CrearPedidoCupoResponse {
  exito: boolean;
  mensaje: string;
  datos: CrearPedidoCupoData | null;
  errores: string[];
}

export async function adquirir_cupo(
  lote_id: number,
  firma: Blob,
): Promise<CrearPedidoCupoResponse> {
  const form = new FormData();
  form.append("loteId", String(lote_id));
  form.append("firma", firma, "firma-conformidad.png");
  return crear_form_data<CrearPedidoCupoResponse>("pedidos/cupo", form);
}

export async function obtener_firma_pedido(pedido_id: string): Promise<Blob> {
  if (!/^\d+$/.test(pedido_id)) {
    throw new Error("El identificador del pedido no es válido.");
  }
  return obtener_blob(`pedidos/${pedido_id}/firma`);
}
