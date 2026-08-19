export interface Proveedor {
  id: string;
  nombre: string;
  descripcion: string;
  verificado: boolean;
}

const proveedores: Proveedor[] = [
  {
    id: "proveedor-001",
    nombre: "Proveedor Demo",
    descripcion: "Proveedor mayorista de tecnología.",
    verificado: true,
  },
  {
    id: "proveedor-002",
    nombre: "Hogar & Tecnología",
    descripcion: "Distribuidor mayorista de productos para el hogar.",
    verificado: true,
  },
  {
    id: "proveedor-003",
    nombre: "Proveedor Nuevo",
    descripcion: "Proveedor recientemente incorporado a SYNER.",
    verificado: false,
  },
];

function obtener_proveedores(): Promise<Proveedor[]> {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(proveedores);
    }, 600);
  });
}

export { proveedores, obtener_proveedores };
