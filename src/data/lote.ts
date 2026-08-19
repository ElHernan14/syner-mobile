export type EstadoLote =
  | "fondeando"
  | "completado"
  | "comprado"
  | "enviado"
  | "entregado"
  | "cancelado";

export interface Lote {
  id: string;
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
  proveedorId?: string;
}

const lotes: Lote[] = [
  {
    id: "lote-001",
    nombre: "Smartphone X",
    descripcion: "Smartphone de última generación.",
    categoria: "Tecnología",
    precioMercado: 1000000,
    precioCupo: 600000,
    porcentajeAhorro: 40,
    cantidadCupos: 10,
    cuposOcupados: 7,
    fechaInicio: "2026-08-01",
    fechaFin: "2026-09-15",
    estado: "fondeando",
    proveedorId: "proveedor-001",
  },
  {
    id: "lote-002",
    nombre: "Notebook Pro 15",
    descripcion: "Notebook profesional para trabajo y estudio.",
    categoria: "Tecnología",
    precioMercado: 1800000,
    precioCupo: 1200000,
    porcentajeAhorro: 33,
    cantidadCupos: 10,
    cuposOcupados: 10,
    fechaInicio: "2026-07-15",
    fechaFin: "2026-08-30",
    estado: "completado",
    proveedorId: "proveedor-001",
  },
  {
    id: "lote-003",
    nombre: 'Smart TV 55"',
    descripcion: "Smart TV 4K de 55 pulgadas.",
    categoria: "Hogar",
    precioMercado: 1200000,
    precioCupo: 720000,
    porcentajeAhorro: 40,
    cantidadCupos: 10,
    cuposOcupados: 0,
    fechaInicio: "2026-08-10",
    fechaFin: "2026-09-25",
    estado: "fondeando",
    proveedorId: "proveedor-002",
  },
];

function obtener_lotes(): Promise<Lote[]> {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(lotes);
    }, 600);
  });
}

export { lotes, obtener_lotes };
