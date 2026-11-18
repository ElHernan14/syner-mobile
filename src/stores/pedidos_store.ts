import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { Pedido } from "@/models/pedido";

import { obtener_pedidos } from "@/services/pedidos_service";
import { useAuthStore } from "@/stores/auth_store";

export const usePedidosStore = defineStore("pedidos", () => {
  const pedidos = ref<Pedido[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);
  const usando_cache = ref(false);
  const auth = useAuthStore();

  const hay_pedidos = computed(() => {
    return pedidos.value.length > 0;
  });

  const page = ref(1);
  const limit = ref(100);
  const total = ref(0);
  const total_paginas = ref(0);

  async function cargar(pagina = 1, pedido_id?: number) {
    cargando.value = true;
    error.value = null;
    usando_cache.value = false;
    const clave = `syner_pedidos_cache:${auth.usuario?.id ?? "anon"}:${auth.rol ?? "sin-rol"}:${pagina}:${pedido_id ?? "todos"}`;

    try {
      const resultado = await obtener_pedidos(
        pagina,
        limit.value,
        undefined,
        undefined,
        pedido_id,
      );

      pedidos.value = resultado.datos;
      localStorage.setItem(clave, JSON.stringify(resultado));

      page.value = resultado.page;
      limit.value = resultado.limit;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
    } catch (err) {
      console.error("Error al cargar pedidos:", err);

      try {
        let cache = JSON.parse(localStorage.getItem(clave) ?? "null");
        if (pedido_id !== undefined && !Array.isArray(cache?.datos)) {
          const clave_lista = `syner_pedidos_cache:${auth.usuario?.id ?? "anon"}:${auth.rol ?? "sin-rol"}:${pagina}:todos`;
          const lista = JSON.parse(localStorage.getItem(clave_lista) ?? "null");
          const pedido = lista?.datos?.find((item: Pedido) => Number(item.id) === pedido_id);
          if (pedido) cache = { ...lista, datos: [pedido] };
        }
        if (Array.isArray(cache?.datos)) {
          pedidos.value = cache.datos;
          page.value = cache.page ?? pagina;
          total.value = cache.total ?? cache.datos.length;
          total_paginas.value = cache.totalPaginas ?? 1;
          usando_cache.value = true;
        } else {
          pedidos.value = [];
          total.value = 0;
          total_paginas.value = 0;
        }
      } catch {
        pedidos.value = [];
        total.value = 0;
        total_paginas.value = 0;
      }

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
    usando_cache,
    hay_pedidos,
    page,
    limit,
    total,
    total_paginas,
    cargar,
  };
});
