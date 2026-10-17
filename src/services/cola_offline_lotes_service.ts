import type { EstadoLote } from "@/models/LoteModel";
import { ApiError } from "@/services/ajax_service";
import { hay_conexion } from "@/services/conectividad_service";
import { cambiar_estado_lote, obtener_lote } from "@/services/lotes_service";

const CLAVE_COLA = "syner_cola_offline_lotes_v1";

export interface OperacionPendienteLote {
  id: string;
  loteId: number;
  estado: EstadoLote;
  usuarioId: string;
  creadaEn: string;
}

type ResultadoSincronizacion = "enviada" | "pendiente" | "rechazada";

function leer_cola(): OperacionPendienteLote[] {
  try {
    const contenido = localStorage.getItem(CLAVE_COLA);
    if (!contenido) return [];

    const operaciones: unknown = JSON.parse(contenido);
    if (!Array.isArray(operaciones)) return [];

    return operaciones.filter(
      (operacion): operacion is OperacionPendienteLote =>
        typeof operacion?.id === "string" &&
        typeof operacion?.loteId === "number" &&
        typeof operacion?.usuarioId === "string" &&
        typeof operacion?.estado === "string" &&
        typeof operacion?.creadaEn === "string",
    );
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

export function obtener_operaciones_pendientes(
  usuario_id?: string,
): OperacionPendienteLote[] {
  const operaciones = leer_cola();

  return usuario_id
    ? operaciones.filter((operacion) => operacion.usuarioId === usuario_id)
    : operaciones;
}

export function encolar_cambio_estado_lote(
  lote_id: number,
  estado: EstadoLote,
  usuario_id: string,
): OperacionPendienteLote {
  const operaciones = leer_cola();

  if (operaciones.some((op) => op.loteId === lote_id)) {
    throw new Error(
      "Este lote ya tiene un cambio pendiente de sincronización.",
    );
  }

  const operacion: OperacionPendienteLote = {
    id: crypto.randomUUID(),
    loteId: lote_id,
    estado,
    usuarioId: usuario_id,
    creadaEn: new Date().toISOString(),
  };

  guardar_cola([...operaciones, operacion]);
  notificar_cambio_cola();

  return operacion;
}

function eliminar_operacion(id: string): void {
  guardar_cola(leer_cola().filter((op) => op.id !== id));
  notificar_cambio_cola();
}

function obtener_status(error: unknown): number | null {
  if (error instanceof ApiError) return error.status;
  return null;
}

function es_fallo_transitorio(error: unknown): boolean {
  if (!(error instanceof Error)) {
    return false;
  }

  if (
    error.message === "No se pudo establecer conexión con la API." ||
    error.message ===
      "Ocurrió un error interno en el servidor. Intentá nuevamente."
  ) {
    return true;
  }

  const status = obtener_status(error);

  return status === 0 || (status !== null && status >= 500);
}

async function enviar_operacion(
  operacion: OperacionPendienteLote,
): Promise<ResultadoSincronizacion> {
  try {
    const respuesta = await cambiar_estado_lote(
      operacion.loteId,
      operacion.estado,
    );

    if (!respuesta.exito || !respuesta.datos) {
      throw new ApiError(
        respuesta.mensaje || "La API rechazó el cambio de estado.",
        400,
        respuesta.errores ?? [],
      );
    }

    return "enviada";
  } catch (error) {
    if (es_fallo_transitorio(error)) {
      return "pendiente";
    }

    // Si se perdió la respuesta del primer intento, el servidor
    // podría haber aplicado el cambio antes de que se cortara la red.
    if (obtener_status(error) === 409) {
      try {
        const lote = await obtener_lote(operacion.loteId);

        if (lote.estado === operacion.estado) {
          return "enviada";
        }
      } catch (error_consulta) {
        if (es_fallo_transitorio(error_consulta)) {
          return "pendiente";
        }
      }
    }

    return "rechazada";
  }
}

export async function sincronizar_cola_lotes(
  usuario_id: string,
): Promise<void> {
  if (!(await hay_conexion())) return;

  const operaciones = leer_cola().filter(
    (operacion) => operacion.usuarioId === usuario_id,
  );

  for (const operacion of operaciones) {
    if (!(await hay_conexion())) break;

    const resultado = await enviar_operacion(operacion);

    if (resultado === "pendiente") {
      // Conservamos esta operación y las siguientes.
      break;
    }

    // Si la API la aceptó o la rechazó definitivamente,
    // no debemos volver a enviarla.
    eliminar_operacion(operacion.id);
  }
}
