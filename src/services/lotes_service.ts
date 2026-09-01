import { obtener } from "@/services/ajax_service";

import type { Lote } from "@/data/lote";

interface LoteApi {
  id: number;
  nombre: string;
  descripcion: string;
  categoria: string;
  precio_mercado: number;
  precio_cupo: number;
  porcentaje_ahorro: number;
  cantidad_cupos: number;
  cupos_ocupados: number;
  fecha_inicio: string;
  fecha_fin: string;
  estado: Lote["estado"];
  proveedor?: {
    id: number;
    nombre: string;
    descripcion: string;
    verificado: boolean;
  };
}

export async function obtener_lotes(): Promise<Lote[]> {
  const respuesta = await obtener<LoteApi[]>("lotes");

  return respuesta.map((lote) => ({
    id: String(lote.id),
    nombre: lote.nombre,
    descripcion: lote.descripcion,
    categoria: lote.categoria,

    precioMercado: lote.precio_mercado,
    precioCupo: lote.precio_cupo,
    porcentajeAhorro: lote.porcentaje_ahorro,

    cantidadCupos: lote.cantidad_cupos,
    cuposOcupados: lote.cupos_ocupados,

    fechaInicio: lote.fecha_inicio,
    fechaFin: lote.fecha_fin,

    estado: lote.estado,

    proveedorId: lote.proveedor ? String(lote.proveedor.id) : undefined,
  }));
}
