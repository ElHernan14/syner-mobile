import { ref } from "vue";
import { defineStore } from "pinia";
import { obtener_lotes } from "@/services/lotes_service";
import type { Lote } from "@/data/lote";

export const useLotesStore = defineStore("lotes", () => {
  const lotes = ref<Lote[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_lotes = ref(false);

  async function cargar() {
    cargando.value = true;
    error.value = null;

    try {
      lotes.value = await obtener_lotes();
      hay_lotes.value = lotes.value.length > 0;
    } catch {
      lotes.value = [];
      hay_lotes.value = false;
      error.value = "No se pudieron cargar los lotes.";
    } finally {
      cargando.value = false;
    }
  }

  return {
    lotes,
    cargando,
    error,
    hay_lotes,
    cargar,
  };
});
