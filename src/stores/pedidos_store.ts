import { defineStore } from "pinia";
import { computed, ref } from "vue";

import type { Pedido } from "@/data/pedido";
import { obtener_pedidos } from "@/services/pedidos_service";

export const usePedidosStore = defineStore("pedidos", () => {
  const pedidos = ref<Pedido[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_pedidos = computed(() => pedidos.value.length > 0);

  async function cargar() {
    cargando.value = true;
    error.value = null;

    try {
      pedidos.value = await obtener_pedidos();
    } catch (err) {
      console.error("Error al cargar pedidos:", err);

      error.value =
        err instanceof Error
          ? err.message
          : "No se pudieron cargar los pedidos.";
    } finally {
      cargando.value = false;
    }
  }

  return {
    pedidos,
    cargando,
    error,
    hay_pedidos,
    cargar,
  };
});
