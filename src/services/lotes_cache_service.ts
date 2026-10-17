import type { Lote } from "@/models/LoteModel";

export interface CacheLotes {
  datos: Lote[];
  page: number;
  limit: number;
  offset: number;
  total: number;
  totalPaginas: number;
}

const PREFIJO_CACHE = "syner_lotes_cache:";
const MAXIMO_CACHES = 15;

export function construir_clave_cache_lotes(
  pagina: number,
  limite: number,
  termino?: string,
  sort?: string,
): string {
  return [
    PREFIJO_CACHE,
    pagina,
    limite,
    encodeURIComponent((termino ?? "").trim().toLowerCase()),
    encodeURIComponent(sort ?? ""),
  ].join(":");
}

export function guardar_cache_lotes(clave: string, datos: CacheLotes): void {
  try {
    localStorage.setItem(
      clave,
      JSON.stringify({
        ...datos,
        guardado_en: new Date().toISOString(),
      }),
    );

    const claves = Object.keys(localStorage).filter((key) =>
      key.startsWith(PREFIJO_CACHE),
    );

    if (claves.length > MAXIMO_CACHES) {
      const clavesOrdenadas = claves
        .map((key) => {
          try {
            const contenido = JSON.parse(localStorage.getItem(key) ?? "{}");
            return {
              key,
              fecha: Date.parse(contenido.guardado_en ?? "") || 0,
            };
          } catch {
            return { key, fecha: 0 };
          }
        })
        .sort((a, b) => a.fecha - b.fecha);

      for (const elemento of clavesOrdenadas.slice(
        0,
        claves.length - MAXIMO_CACHES,
      )) {
        localStorage.removeItem(elemento.key);
      }
    }
  } catch (error) {
    // Si el almacenamiento está lleno o no disponible,
    // la consulta online debe seguir funcionando.
    console.warn("No se pudo guardar el caché de lotes.", error);
  }
}

export function obtener_cache_lotes(clave: string): CacheLotes | null {
  try {
    const contenido = localStorage.getItem(clave);

    if (!contenido) return null;

    const cache = JSON.parse(contenido) as CacheLotes;

    if (
      !Array.isArray(cache.datos) ||
      typeof cache.page !== "number" ||
      typeof cache.limit !== "number" ||
      typeof cache.total !== "number" ||
      typeof cache.totalPaginas !== "number"
    ) {
      localStorage.removeItem(clave);
      return null;
    }

    return cache;
  } catch {
    return null;
  }
}
