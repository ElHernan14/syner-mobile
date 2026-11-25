import { obtener, crear, actualizar } from "@/services/ajax_service";
import type { ApiResponse } from "@/models/api_response";
import type { Direccion, GuardarDireccionRequest } from "@/models/direccion";

function clave_cache(usuario_id: string): string {
  return `syner_direccion_principal_cache:${usuario_id}`;
}

export function obtener_direccion_cache(usuario_id: string): Direccion | null {
  try {
    const cache: unknown = JSON.parse(localStorage.getItem(clave_cache(usuario_id)) ?? "null");
    if (
      typeof cache !== "object" ||
      cache === null ||
      !("id" in cache) ||
      typeof cache.id !== "number" ||
      !("calle" in cache) ||
      typeof cache.calle !== "string" ||
      !("numero" in cache) ||
      typeof cache.numero !== "string" ||
      !("ciudad" in cache) ||
      typeof cache.ciudad !== "string" ||
      !("provincia" in cache) ||
      typeof cache.provincia !== "string" ||
      !("codigoPostal" in cache) ||
      typeof cache.codigoPostal !== "string" ||
      !("referencia" in cache) ||
      (cache.referencia !== null && typeof cache.referencia !== "string") ||
      !("latitud" in cache) ||
      typeof cache.latitud !== "number" ||
      !("longitud" in cache) ||
      typeof cache.longitud !== "number" ||
      !("esPrincipal" in cache) ||
      typeof cache.esPrincipal !== "boolean" ||
      !("fechaCreacion" in cache) ||
      typeof cache.fechaCreacion !== "string" ||
      !("fechaActualizacion" in cache) ||
      typeof cache.fechaActualizacion !== "string"
    ) {
      return null;
    }
    return cache as Direccion;
  } catch (error) {
    console.warn("No se pudo leer el caché de la dirección.", error);
    return null;
  }
}

function guardar_direccion_cache(usuario_id: string, direccion: Direccion | null): void {
  try {
    if (direccion) localStorage.setItem(clave_cache(usuario_id), JSON.stringify(direccion));
    else localStorage.removeItem(clave_cache(usuario_id));
  } catch (error) {
    console.warn("No se pudo guardar el caché de la dirección.", error);
  }
}

export async function obtener_mi_direccion(): Promise<Direccion | null> {
  const respuesta = await obtener<ApiResponse<Direccion | null>>("direcciones/mi-direccion");
  if (!respuesta.exito) {
    throw new Error(respuesta.errores?.join(" ") || respuesta.mensaje || "No se pudo consultar la dirección.");
  }
  return respuesta.datos;
}

export async function crear_direccion(
  request: GuardarDireccionRequest,
): Promise<Direccion> {
  const respuesta = await crear<ApiResponse<Direccion>>("direcciones", request);
  if (!respuesta.exito || !respuesta.datos) {
    throw new Error(respuesta.errores?.join(" ") || respuesta.mensaje || "No se pudo guardar la dirección.");
  }
  return respuesta.datos;
}

export async function actualizar_direccion(
  request: GuardarDireccionRequest,
): Promise<Direccion> {
  const respuesta = await actualizar<ApiResponse<Direccion>>("direcciones/mi-direccion", request);
  if (!respuesta.exito || !respuesta.datos) {
    throw new Error(respuesta.errores?.join(" ") || respuesta.mensaje || "No se pudo actualizar la dirección.");
  }
  return respuesta.datos;
}

export function actualizar_cache_direccion(usuario_id: string, direccion: Direccion | null): void {
  guardar_direccion_cache(usuario_id, direccion);
}
