import { obtener } from "@/services/ajax_service";

export interface HistorialLoteItem {
  id: number;
  lote_id: number;
  lote: string;
  usuario_id: number;
  usuario: string;
  estado_anterior: string;
  nuevo_estado: string;
  fecha_modificacion: string;
}

interface RespuestaHistorial {
  exito: boolean;
  mensaje: string;
  datos: { datos: HistorialLoteItem[]; page: number; limit: number; total: number; totalPaginas: number } | null;
  errores: string[];
}

export interface HistorialLotesPaginado {
  datos: HistorialLoteItem[];
  page: number;
  limit: number;
  total: number;
  totalPaginas: number;
}

function es_item_historial(valor: unknown): valor is HistorialLoteItem {
  if (typeof valor !== "object" || valor === null) return false;
  return "id" in valor && typeof valor.id === "number" &&
    "lote_id" in valor && typeof valor.lote_id === "number" &&
    "lote" in valor && typeof valor.lote === "string" &&
    "usuario_id" in valor && typeof valor.usuario_id === "number" &&
    "usuario" in valor && typeof valor.usuario === "string" &&
    "estado_anterior" in valor && typeof valor.estado_anterior === "string" &&
    "nuevo_estado" in valor && typeof valor.nuevo_estado === "string" &&
    "fecha_modificacion" in valor && typeof valor.fecha_modificacion === "string";
}

function clave_cache(usuario_id: string, page: number, limit: number): string {
  return `syner_historial_lotes_cache:${usuario_id}:${page}:${limit}`;
}

export function guardar_cache_historial_lotes(
  usuario_id: string,
  page: number,
  limit: number,
  resultado: HistorialLotesPaginado,
): void {
  try {
    localStorage.setItem(clave_cache(usuario_id, page, limit), JSON.stringify(resultado));
  } catch (error) {
    console.warn("No se pudo guardar el caché del historial de lotes.", error);
  }
}

export function obtener_cache_historial_lotes(
  usuario_id: string,
  page: number,
  limit: number,
): HistorialLotesPaginado | null {
  try {
    const contenido: unknown = JSON.parse(
      localStorage.getItem(clave_cache(usuario_id, page, limit)) ?? "null",
    );
    if (
      typeof contenido !== "object" ||
      contenido === null ||
      !("datos" in contenido) ||
      !Array.isArray(contenido.datos) ||
      !contenido.datos.every(es_item_historial) ||
      !("page" in contenido) ||
      typeof contenido.page !== "number" ||
      !("limit" in contenido) ||
      typeof contenido.limit !== "number" ||
      !("total" in contenido) ||
      typeof contenido.total !== "number" ||
      !("totalPaginas" in contenido) ||
      typeof contenido.totalPaginas !== "number"
    ) {
      return null;
    }
    return {
      datos: contenido.datos,
      page: contenido.page,
      limit: contenido.limit,
      total: contenido.total,
      totalPaginas: contenido.totalPaginas,
    };
  } catch (error) {
    console.warn("No se pudo leer el caché del historial de lotes.", error);
    return null;
  }
}

export async function obtener_historial_lotes(page = 1, limit = 20) {
  const respuesta = await obtener<RespuestaHistorial>(`historial/lotes?page=${page}&limit=${limit}`);
  if (!respuesta.exito || !respuesta.datos) throw new Error(respuesta.errores?.join(" ") || respuesta.mensaje || "No se pudo cargar el historial.");
  return respuesta.datos;
}
