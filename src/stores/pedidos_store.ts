import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { Pedido } from "@/models/pedido";

import { obtener_pedidos } from "@/services/pedidos_service";

export const usePedidosStore = defineStore("pedidos", () => {
  const pedidos = ref<Pedido[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_pedidos = computed(() => {
    return pedidos.value.length > 0;
  });

  const page = ref(1);
  const limit = ref(10);
  const total = ref(0);
  const total_paginas = ref(0);

  async function cargar(pagina = 1) {
    cargando.value = true;
    error.value = null;

    try {
      const resultado = await obtener_pedidos(pagina, limit.value);

      pedidos.value = resultado.datos;

      page.value = resultado.page;
      limit.value = resultado.limit;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
    } catch (err) {
      console.error("Error al cargar pedidos:", err);

      pedidos.value = [];
      total.value = 0;
      total_paginas.value = 0;

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
    page,
    limit,
    total,
    total_paginas,
    cargar,
  };
});
