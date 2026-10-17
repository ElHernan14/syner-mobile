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
    operaciones_pendientes,
    usando_cache,
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

  function cambiar_estado_lote_local(
    id: number,
    estado: Parameters<typeof lotes_store.cambiar_estado>[1],
    usuario_id: string,
  ) {
    return lotes_store.cambiar_estado(id, estado, usuario_id);
  }

  function refrescar_operaciones_pendientes() {
    lotes_store.refrescar_operaciones_pendientes();
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
    operaciones_pendientes,
    cambiar_estado_lote_local,
    refrescar_operaciones_pendientes,
    usando_cache,
  };
}
