export type EstadoLote =
  | "borrador"
  | "fondeando"
  | "completado"
  | "comprado"
  | "enviado"
  | "entregado"
  | "cancelado";

export interface ProveedorLote {
  id: number;
  nombre: string;
  descripcion: string;
  verificado: boolean;
}

export interface Lote {
  id: number;
  nombre: string;
  descripcion: string;
  categoria: string;
  precioMercado: number;
  precioCupo: number;
  porcentajeAhorro: number;
  cantidadCupos: number;
  cuposOcupados: number;
  fechaInicio: string;
  fechaFin: string;
  estado: EstadoLote;
  proveedorId: number;
  proveedor: ProveedorLote;
}
