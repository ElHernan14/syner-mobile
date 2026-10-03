import { actualizar, obtener } from "@/services/ajax_service";

import type { Notificacion } from "@/models/notificacion";

interface RespuestaNotificaciones {
  exito: boolean;
  mensaje: string;
  datos: Notificacion[] | null;
  errores: string[];
}

interface RespuestaMarcarLeida {
  exito: boolean;
  mensaje: string;
  datos: {
    id: number;
    leida: boolean;
  } | null;
  errores: string[];
}

export async function obtener_notificaciones(): Promise<RespuestaNotificaciones> {
  return obtener<RespuestaNotificaciones>("notificaciones");
}

export async function obtener_no_leidas(): Promise<RespuestaNotificaciones> {
  return obtener<RespuestaNotificaciones>("notificaciones/no-leidas");
}

export async function marcar_como_leida(
  id: number,
): Promise<RespuestaMarcarLeida> {
  return actualizar<RespuestaMarcarLeida>(`notificaciones/${id}/leer`, {});
}
