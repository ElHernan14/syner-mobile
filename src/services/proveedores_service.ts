import { obtener } from "@/services/ajax_service";

import type { Proveedor } from "@/models/proveedor";

const CLAVE_CACHE_PROVEEDORES = "syner_proveedores_cache_v1";

export function obtener_proveedores_cache(): Proveedor[] {
  try {
    const datos = JSON.parse(localStorage.getItem(CLAVE_CACHE_PROVEEDORES) ?? "[]");
    return Array.isArray(datos) ? datos : [];
  } catch {
    return [];
  }
}

interface ProveedorApi {
  id: number;
  nombre: string;
  descripcion: string;
  verificado: boolean;
}

export async function obtener_proveedores(): Promise<Proveedor[]> {
  const respuesta = await obtener<ProveedorApi[]>("proveedores");

  const proveedores = respuesta.map((proveedor) => ({
    id: String(proveedor.id),
    nombre: proveedor.nombre,
    descripcion: proveedor.descripcion,
    verificado: proveedor.verificado,
  }));
  try {
    localStorage.setItem(CLAVE_CACHE_PROVEEDORES, JSON.stringify(proveedores));
  } catch {
    // El listado remoto sigue disponible aunque el almacenamiento esté lleno.
  }
  return proveedores;
}
