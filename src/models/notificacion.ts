export type TipoNotificacion =
  | "pedido_nuevo"
  | "pedido_creado"
  | "lote_disponible"
  | "lote_actualizado"
  | "otro";

export interface Notificacion {
  id: number;
  tipo: TipoNotificacion | string;
  titulo: string;
  mensaje: string;
  leida: boolean;
  fechaCreacion: string;
  entidadTipo?: string | null;
  entidadId?: number | null;
}
