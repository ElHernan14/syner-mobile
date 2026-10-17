import {
  alertCircleOutline,
  checkmarkCircleOutline,
  cubeOutline,
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
};

export function obtener_estado_visual(estado: string): EstadoVisual {
  return estados[estado.toLowerCase()] ?? {
    etiqueta: estado,
    color: "medium",
    icono: alertCircleOutline,
  };
}
