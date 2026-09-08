import { obtener } from "@/services/ajax_service";

import type { Proveedor } from "@/models/proveedor";

interface ProveedorApi {
  id: number;
  nombre: string;
  descripcion: string;
  verificado: boolean;
}

export async function obtener_proveedores(): Promise<Proveedor[]> {
  const respuesta = await obtener<ProveedorApi[]>("proveedores");

  return respuesta.map((proveedor) => ({
    id: String(proveedor.id),
    nombre: proveedor.nombre,
    descripcion: proveedor.descripcion,
    verificado: proveedor.verificado,
  }));
}
