import { storeToRefs } from "pinia";

import { useLotesStore } from "@/stores/lotes_store";

export function useLotes() {
  const lotes_store = useLotesStore();

  const {
    lotes,
    cargando,
    procesando,
    error,
    hay_lotes,
    page,
    limit,
    total,
    total_paginas,
  } = storeToRefs(lotes_store);

  function cargar_lotes(pagina = 1, termino?: string, sort?: string) {
    return lotes_store.cargar(pagina, termino, sort);
  }

  function crear_lote(request: Parameters<typeof lotes_store.crear>[0]) {
    return lotes_store.crear(request);
  }

  function actualizar_lote(
    id: number,
    request: Parameters<typeof lotes_store.actualizar>[1],
  ) {
    return lotes_store.actualizar(id, request);
  }

  function eliminar_lote(id: number) {
    return lotes_store.eliminar(id);
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
    cargar_lotes,
    crear_lote,
    actualizar_lote,
    eliminar_lote,
  };
}
