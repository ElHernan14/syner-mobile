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

export async function obtener_historial_lotes(page = 1, limit = 20) {
  const respuesta = await obtener<RespuestaHistorial>(`historial/lotes?page=${page}&limit=${limit}`);
  if (!respuesta.exito || !respuesta.datos) throw new Error(respuesta.errores?.join(" ") || respuesta.mensaje || "No se pudo cargar el historial.");
  return respuesta.datos;
}
