import { storeToRefs } from "pinia";
import { useLotesStore } from "@/stores/lotes_store";

export function useLotes() {
  const lotes_store = useLotesStore();

  const { lotes, cargando, error, hay_lotes } = storeToRefs(lotes_store);

  function cargar_lotes() {
    return lotes_store.cargar();
  }

  return {
    lotes,
    cargando,
    error,
    hay_lotes,
    cargar_lotes,
  };
}
