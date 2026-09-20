export interface ResultadoPaginado<T> {
  datos: T[];
  page: number;
  limit: number;
  offset: number;
  total: number;
  totalPaginas: number;
}
