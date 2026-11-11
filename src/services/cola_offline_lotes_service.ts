import type { EstadoLote } from "@/models/LoteModel";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";
import { ApiError } from "@/services/ajax_service";
import { hay_conexion } from "@/services/conectividad_service";
import {
  actualizar_lote,
  cambiar_estado_lote,
  crear_lote,
  obtener_lote,
} from "@/services/lotes_service";

const CLAVE_COLA = "syner_cola_offline_lotes_v1";

interface OperacionBase {
  id: string;
  usuarioId: string;
  creadaEn: string;
}

export type OperacionPendienteLote =
  | (OperacionBase & { tipo: "estado"; loteId: number; estado: EstadoLote })
  | (OperacionBase & { tipo: "crear"; loteId: number; request: CrearLoteRequest })
  | (OperacionBase & { tipo: "actualizar"; loteId: number; request: ActualizarLoteRequest });

type ResultadoSincronizacion = "enviada" | "pendiente" | "rechazada";

function normalizar_operacion(valor: unknown): OperacionPendienteLote | null {
  if (!valor || typeof valor !== "object") return null;
  const op = valor as Record<string, unknown>;
  if (
    typeof op.id !== "string" || typeof op.loteId !== "number" ||
    typeof op.usuarioId !== "string" || typeof op.creadaEn !== "string"
  ) return null;

  // Compatibilidad con la cola anterior, que guardaba solo cambios de estado.
  const tipo = op.tipo ?? "estado";
  if (tipo === "estado" && typeof op.estado === "string") {
    return { id: op.id, loteId: op.loteId, usuarioId: op.usuarioId, creadaEn: op.creadaEn, tipo, estado: op.estado as EstadoLote };
  }
  if (tipo === "crear" && op.request && typeof op.request === "object") {
    return { id: op.id, loteId: op.loteId, usuarioId: op.usuarioId, creadaEn: op.creadaEn, tipo, request: op.request as CrearLoteRequest };
  }
  if (tipo === "actualizar" && op.request && typeof op.request === "object") {
    return { id: op.id, loteId: op.loteId, usuarioId: op.usuarioId, creadaEn: op.creadaEn, tipo, request: op.request as ActualizarLoteRequest };
  }
  return null;
}

function leer_cola(): OperacionPendienteLote[] {
  try {
    const contenido = localStorage.getItem(CLAVE_COLA);
    if (!contenido) return [];
    const operaciones: unknown = JSON.parse(contenido);
    return Array.isArray(operaciones)
      ? operaciones.map(normalizar_operacion).filter((op): op is OperacionPendienteLote => op !== null)
      : [];
  } catch {
    return [];
  }
}

function guardar_cola(operaciones: OperacionPendienteLote[]): void {
  localStorage.setItem(CLAVE_COLA, JSON.stringify(operaciones));
}

function notificar_cambio_cola(): void {
  window.dispatchEvent(new CustomEvent("syner:cola-lotes-actualizada"));
}

export function obtener_operaciones_pendientes(usuario_id?: string): OperacionPendienteLote[] {
  const operaciones = leer_cola();
  return usuario_id ? operaciones.filter((op) => op.usuarioId === usuario_id) : operaciones;
}

function guardar_operacion(operacion: OperacionPendienteLote, reemplazar = false): void {
  const operaciones = leer_cola();
  const indice = operaciones.findIndex((op) => op.loteId === operacion.loteId && op.usuarioId === operacion.usuarioId);
  if (indice >= 0) {
    if (!reemplazar && operacion.tipo === "estado") {
      throw new Error("Este lote ya tiene un cambio pendiente de sincronización.");
    }
    // Una edición local posterior a un alta todavía no enviada actualiza ese POST.
    if (operacion.tipo === "actualizar" && operaciones[indice].tipo === "crear") {
      operaciones[indice] = { ...operaciones[indice], request: operacion.request as CrearLoteRequest };
    } else {
      operaciones[indice] = operacion;
    }
  } else {
    operaciones.push(operacion);
  }
  guardar_cola(operaciones);
  notificar_cambio_cola();
}

function crear_base(usuario_id: string): OperacionBase {
  return { id: crypto.randomUUID(), usuarioId: usuario_id, creadaEn: new Date().toISOString() };
}

export function encolar_cambio_estado_lote(lote_id: number, estado: EstadoLote, usuario_id: string): OperacionPendienteLote {
  const operacion: OperacionPendienteLote = { ...crear_base(usuario_id), tipo: "estado", loteId: lote_id, estado };
  guardar_operacion(operacion);
  return operacion;
}

export function encolar_creacion_lote(lote_id: number, request: CrearLoteRequest, usuario_id: string): void {
  guardar_operacion({ ...crear_base(usuario_id), tipo: "crear", loteId: lote_id, request });
}

export function encolar_actualizacion_lote(lote_id: number, request: ActualizarLoteRequest, usuario_id: string): void {
  guardar_operacion({ ...crear_base(usuario_id), tipo: "actualizar", loteId: lote_id, request }, true);
}

function eliminar_operacion(id: string): void {
  guardar_cola(leer_cola().filter((op) => op.id !== id));
  notificar_cambio_cola();
}

function obtener_status(error: unknown): number | null {
  return error instanceof ApiError ? error.status : null;
}

function es_fallo_transitorio(error: unknown): boolean {
  if (!(error instanceof Error)) return false;
  if (error.message === "No se pudo establecer conexión con la API." || error.message === "Ocurrió un error interno en el servidor. Intentá nuevamente.") return true;
  const status = obtener_status(error);
  return status === 0 || (status !== null && status >= 500);
}

async function enviar_operacion(operacion: OperacionPendienteLote): Promise<ResultadoSincronizacion> {
  try {
    if (operacion.tipo === "estado") {
      const respuesta = await cambiar_estado_lote(operacion.loteId, operacion.estado);
      if (!respuesta.exito || !respuesta.datos) throw new ApiError(respuesta.mensaje || "La API rechazó el cambio de estado.", 400, respuesta.errores ?? []);
    } else if (operacion.tipo === "crear") {
      await crear_lote(operacion.request);
    } else {
      await actualizar_lote(operacion.loteId, operacion.request);
    }
    return "enviada";
  } catch (error) {
    if (es_fallo_transitorio(error)) return "pendiente";
    if (operacion.tipo === "estado" && obtener_status(error) === 409) {
      try {
        if ((await obtener_lote(operacion.loteId)).estado === operacion.estado) return "enviada";
      } catch (error_consulta) {
        if (es_fallo_transitorio(error_consulta)) return "pendiente";
      }
    }
    return "rechazada";
  }
}

export async function sincronizar_cola_lotes(usuario_id: string): Promise<void> {
  if (!(await hay_conexion())) return;
  const operaciones = leer_cola().filter((op) => op.usuarioId === usuario_id);
  for (const operacion of operaciones) {
    if (!(await hay_conexion())) break;
    const resultado = await enviar_operacion(operacion);
    if (resultado === "pendiente") break;
    eliminar_operacion(operacion.id);
  }
}
