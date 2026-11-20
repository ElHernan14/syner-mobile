export interface CrearLoteRequest {
  nombre: string;
  descripcion: string;
  categoria: string;
  precioMercado: number;
  precioCupo: number;
  cantidadCupos: number;
  fechaInicio: string;
  fechaFin: string;
  proveedorId: number;
  imagenAvatar?: string | null;
}
