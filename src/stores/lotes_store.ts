import { ref } from "vue";
import { defineStore } from "pinia";
import { useAuthStore } from "@/stores/auth_store";

import {
  obtener_lotes,
  crear_lote,
  actualizar_lote,
  eliminar_lote,
  cambiar_estado_lote,
} from "@/services/lotes_service";

import type { Lote, EstadoLote } from "@/models/LoteModel";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";
import {
  construir_clave_cache_lotes,
  guardar_cache_lotes,
  obtener_cache_lotes,
} from "@/services/lotes_cache_service";
import { ApiError } from "@/services/ajax_service";
import {
  encolar_cambio_estado_lote,
  obtener_operaciones_pendientes,
  type OperacionPendienteLote,
} from "@/services/cola_offline_lotes_service";
import { hay_conexion } from "@/services/conectividad_service";

function obtener_mensaje_error(
  error: unknown,
  mensaje_generico: string,
): string {
  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return mensaje_generico;
}

export const useLotesStore = defineStore("lotes", () => {
  const auth_store = useAuthStore();
  const lotes = ref<Lote[]>([]);
  const cargando = ref(false);
  const procesando = ref(false);
  const error = ref<string | null>(null);
  const hay_lotes = ref(false);

  const page = ref(1);
  const limit = ref(10);
  const total = ref(0);
  const total_paginas = ref(0);
  const usando_cache = ref(false);
  const operaciones_pendientes = ref<OperacionPendienteLote[]>(
    obtener_operaciones_pendientes(auth_store.usuario?.id),
  );

  function aplicar_cambios_pendientes(lotes_a_mostrar: Lote[]): Lote[] {
    const estados_pendientes = new Map(
      obtener_operaciones_pendientes(auth_store.usuario?.id).map(
        (operacion) => [operacion.loteId, operacion.estado],
      ),
    );

    return lotes_a_mostrar.map((lote) => {
      const estado_pendiente = estados_pendientes.get(lote.id);

      return estado_pendiente ? { ...lote, estado: estado_pendiente } : lote;
    });
  }

  async function cargar(pagina = 1, termino?: string, sort?: string) {
    cargando.value = true;
    error.value = null;
    usando_cache.value = false;

    const clave_cache = construir_clave_cache_lotes(
      pagina,
      limit.value,
      termino,
      sort,
    );

    try {
      const resultado = await obtener_lotes(pagina, limit.value, termino, sort);

      lotes.value = aplicar_cambios_pendientes(resultado.datos);
      page.value = resultado.page;
      limit.value = resultado.limit;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
      hay_lotes.value = resultado.datos.length > 0;

      guardar_cache_lotes(clave_cache, resultado);
    } catch (err) {
      const cache = obtener_cache_lotes(clave_cache);

      error.value = obtener_mensaje_error(
        err,
        "No se pudieron cargar los lotes.",
      );

      if (cache) {
        lotes.value = aplicar_cambios_pendientes(cache.datos);
        page.value = cache.page;
        limit.value = cache.limit;
        total.value = cache.total;
        total_paginas.value = cache.totalPaginas;
        hay_lotes.value = cache.datos.length > 0;
        usando_cache.value = true;
      } else {
        lotes.value = [];
        hay_lotes.value = false;
        total.value = 0;
        total_paginas.value = 0;
      }
    } finally {
      cargando.value = false;
    }
  }

  async function crear(request: CrearLoteRequest) {
    procesando.value = true;
    error.value = null;

    try {
      const lote = await crear_lote(request);

      lotes.value.push(lote);
      hay_lotes.value = true;

      return lote;
    } catch (err) {
      const mensaje = obtener_mensaje_error(err, "No se pudo crear el lote.");

      error.value = mensaje;

      throw new Error(mensaje);
    } finally {
      procesando.value = false;
    }
  }

  async function actualizar(id: number, request: ActualizarLoteRequest) {
    procesando.value = true;
    error.value = null;

    try {
      const lote = await actualizar_lote(id, request);

      const indice = lotes.value.findIndex((item) => item.id === id);

      if (indice !== -1) {
        lotes.value[indice] = lote;
      }

      return lote;
    } catch (err) {
      const mensaje = obtener_mensaje_error(
        err,
        "No se pudo actualizar el lote.",
      );

      error.value = mensaje;

      throw new Error(mensaje);
    } finally {
      procesando.value = false;
    }
  }

  async function eliminar(id: number) {
    procesando.value = true;
    error.value = null;

    try {
      await eliminar_lote(id);

      const indice = lotes.value.findIndex((item) => item.id === id);

      if (indice !== -1) {
        lotes.value[indice].estado = "cancelado";
      }
    } catch (err) {
      const mensaje = obtener_mensaje_error(
        err,
        "No se pudo dar de baja el lote.",
      );

      error.value = mensaje;

      throw new Error(mensaje);
    } finally {
      procesando.value = false;
    }
  }

  function actualizar_estado_local(lote_id: number, estado: EstadoLote): void {
    const lote = lotes.value.find((item) => item.id === lote_id);

    if (lote) {
      lote.estado = estado;
    }

    hay_lotes.value = lotes.value.length > 0;
  }

  function refrescar_operaciones_pendientes(): void {
    operaciones_pendientes.value = obtener_operaciones_pendientes(
      auth_store.usuario?.id,
    );
  }

  function es_fallo_temporal(error: unknown): boolean {
    if (
      error instanceof Error &&
      error.message === "No se pudo establecer conexión con la API."
    ) {
      return true;
    }

    if (!(error instanceof ApiError)) {
      return false;
    }

    return error.status === 0 || error.status >= 500;
  }

  async function cambiar_estado(
    lote_id: number,
    estado: EstadoLote,
    usuario_id: string,
  ): Promise<{ pendiente: boolean }> {
    const lote = lotes.value.find((item) => item.id === lote_id);

    if (!lote) {
      throw new Error("No se encontró el lote que querés modificar.");
    }

    if (!usuario_id.trim()) {
      throw new Error("No se pudo identificar al administrador.");
    }

    // Sin conexión: guardamos primero y actualizamos la interfaz.
    if (!(await hay_conexion())) {
      encolar_cambio_estado_lote(lote_id, estado, usuario_id);
      actualizar_estado_local(lote_id, estado);
      refrescar_operaciones_pendientes();

      return { pendiente: true };
    }

    try {
      const respuesta = await cambiar_estado_lote(lote_id, estado);

      if (!respuesta.exito || !respuesta.datos) {
        throw new Error(
          respuesta.mensaje || "La API no confirmó el cambio de estado.",
        );
      }

      actualizar_estado_local(lote_id, estado);

      return { pendiente: false };
    } catch (error) {
      // Si la red cae durante la solicitud, conservamos el cambio.
      // No encolamos los rechazos definitivos de la API.
      if (!es_fallo_temporal(error)) {
        throw error;
      }

      encolar_cambio_estado_lote(lote_id, estado, usuario_id);
      actualizar_estado_local(lote_id, estado);
      refrescar_operaciones_pendientes();

      return { pendiente: true };
    }
  }

  return {
    lotes,
    cargando,
    procesando,
    error,
    hay_lotes,
    page,
    limit,
    total,
    total_paginas,
    cargar,
    crear,
    actualizar,
    eliminar,
    usando_cache,
    operaciones_pendientes,
    cambiar_estado,
    refrescar_operaciones_pendientes,
  };
});
