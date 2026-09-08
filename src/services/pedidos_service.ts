import { obtener } from "@/services/ajax_service";

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

export async function obtener_pedidos(): Promise<Pedido[]> {
  const respuesta = await obtener<PedidoApi[]>("pedidos");

  return respuesta.map((pedido) => ({
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
  }));
}
