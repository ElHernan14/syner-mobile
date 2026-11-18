import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { Usuario } from "@/models/usuario";

import { obtener_usuarios } from "@/services/usuarios_service";
import { useAuthStore } from "@/stores/auth_store";

export const useUsuariosStore = defineStore("usuarios", () => {
  const usuarios = ref<Usuario[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);
  const usando_cache = ref(false);
  const auth = useAuthStore();

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
    usando_cache.value = false;
    const clave = `syner_usuarios_cache:${auth.usuario?.id ?? "anon"}:${pagina}`;

    try {
      const resultado = await obtener_usuarios(pagina, limit.value);

      usuarios.value = resultado.datos;
      localStorage.setItem(clave, JSON.stringify(resultado));

      page.value = resultado.page;
      limit.value = resultado.limit;
      total.value = resultado.total;
      total_paginas.value = resultado.totalPaginas;
    } catch (err) {
      console.error("Error al cargar usuarios:", err);

      try {
        const cache = JSON.parse(localStorage.getItem(clave) ?? "null");
        if (Array.isArray(cache?.datos)) {
          usuarios.value = cache.datos;
          page.value = cache.page ?? pagina;
          total.value = cache.total ?? cache.datos.length;
          total_paginas.value = cache.totalPaginas ?? 1;
          usando_cache.value = true;
        } else {
          usuarios.value = [];
          total.value = 0;
          total_paginas.value = 0;
        }
      } catch {
        usuarios.value = [];
        total.value = 0;
        total_paginas.value = 0;
      }

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
    usando_cache,
    hay_usuarios,
    page,
    limit,
    total,
    total_paginas,
    cargar,
  };
});
