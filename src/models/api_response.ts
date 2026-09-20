export interface ApiResponse<T> {
  exito: boolean;
  mensaje: string;
  datos: T | null;
  errores: string[];
}
