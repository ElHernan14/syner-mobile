export interface ActualizarLoteRequest {
  nombre: string;
  descripcion: string;
  categoria: string;
  precioMercado: number;
  precioCupo: number;
  porcentajeAhorro: number;
  cantidadCupos: number;
  fechaInicio: string;
  fechaFin: string;
  proveedorId: number;
}
