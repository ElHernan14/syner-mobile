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

const etiquetas_estado_api: Record<number, string> = {
  0: "borrador",
  1: "fondeando",
  2: "completado",
  3: "comprado",
  4: "enviado",
  5: "entregado",
  6: "cancelado",
};

export function obtener_estado_visual(estado: unknown): EstadoVisual {
  const valor = typeof estado === "string"
    ? estado.trim()
    : typeof estado === "number"
      ? etiquetas_estado_api[estado] ?? String(estado)
      : "";
  const estado_normalizado = valor.toLowerCase();
  return estados[estado_normalizado] ?? {
    etiqueta: valor || "Sin estado",
    color: "medium",
    icono: alertCircleOutline,
  };
}
