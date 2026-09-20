import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { Usuario } from "@/models/usuario";

import { obtener_usuarios } from "@/services/usuarios_service";

export const useUsuariosStore = defineStore("usuarios", () => {
  const usuarios = ref<Usuario[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_usuarios = computed(() => {
    return usuarios.value.length > 0;
  });

  const page = ref(1);
  const limit = ref(10);
  const total = ref(0);
  const total_paginas = ref(0);

  async function cargar(pagina = 1) {
    cargando.value = true;
    error.value = null;

    try {
      const resultado = await obtener_usuarios(pagina, limit.value);

      usuarios.value = resultado.datos;

      page.value = resultado.page;
      limit.value = resultado.limit;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
    } catch (err) {
      console.error("Error al cargar usuarios:", err);

      usuarios.value = [];
      total.value = 0;
      total_paginas.value = 0;

      error.value =
        err instanceof Error
          ? err.message
          : "No se pudieron cargar los usuarios.";
    } finally {
      cargando.value = false;
    }
  }

  return {
    usuarios,
    cargando,
    error,
    hay_usuarios,
    page,
    limit,
    total,
    total_paginas,
    cargar,
  };
});
