import { ref } from "vue";
import { defineStore } from "pinia";

import {
  obtener_lotes,
  crear_lote,
  actualizar_lote,
  eliminar_lote,
} from "@/services/lotes_service";

import type { Lote } from "@/models/LoteModel";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";

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
  const lotes = ref<Lote[]>([]);
  const cargando = ref(false);
  const procesando = ref(false);
  const error = ref<string | null>(null);
  const hay_lotes = ref(false);

  const page = ref(1);
  const limit = ref(10);
  const total = ref(0);
  const total_paginas = ref(0);

  async function cargar(pagina = 1, termino?: string, sort?: string) {
    cargando.value = true;
    error.value = null;

    try {
      const resultado = await obtener_lotes(pagina, limit.value, termino, sort);

      lotes.value = resultado.datos;
      page.value = resultado.page;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
      hay_lotes.value = lotes.value.length > 0;
    } catch (err) {
      lotes.value = [];
      hay_lotes.value = false;
      total.value = 0;
      total_paginas.value = 0;

      error.value = obtener_mensaje_error(
        err,
        "No se pudieron cargar los lotes.",
      );
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
  };
});
