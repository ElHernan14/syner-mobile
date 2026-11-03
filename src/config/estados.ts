import {
  alertCircleOutline,
  checkmarkCircleOutline,
  cubeOutline,
  ellipseOutline,
  personOutline,
  sendOutline,
  timeOutline,
} from "ionicons/icons";

export type EstadoVisual = {
  etiqueta: string;
  color: "medium" | "primary" | "success" | "tertiary" | "warning" | "danger";
  icono: string;
};

const estados: Record<string, EstadoVisual> = {
  borrador: { etiqueta: "Borrador", color: "medium", icono: timeOutline },
  fondeando: { etiqueta: "Fondeando", color: "primary", icono: timeOutline },
  completado: { etiqueta: "Completado", color: "success", icono: checkmarkCircleOutline },
  comprado: { etiqueta: "Comprado", color: "tertiary", icono: cubeOutline },
  enviado: { etiqueta: "Enviado", color: "warning", icono: sendOutline },
  entregado: { etiqueta: "Entregado", color: "success", icono: checkmarkCircleOutline },
  cancelado: { etiqueta: "Cancelado", color: "danger", icono: alertCircleOutline },
  pendiente: { etiqueta: "Pendiente", color: "warning", icono: timeOutline },
  verificado: { etiqueta: "Verificado", color: "success", icono: checkmarkCircleOutline },
  bloqueado: { etiqueta: "Bloqueado", color: "danger", icono: alertCircleOutline },
  activo: { etiqueta: "Activo", color: "success", icono: checkmarkCircleOutline },
  inactivo: { etiqueta: "Inactivo", color: "medium", icono: ellipseOutline },
  usuario: { etiqueta: "Usuario", color: "primary", icono: personOutline },
  "sin verificar": { etiqueta: "Sin verificar", color: "warning", icono: alertCircleOutline },
};

export function obtener_estado_visual(estado: string): EstadoVisual {
  const estado_normalizado = estado.trim().toLowerCase();
  return estados[estado_normalizado] ?? {
    etiqueta: estado.trim() || "Sin estado",
    color: "medium",
    icono: alertCircleOutline,
  };
}
