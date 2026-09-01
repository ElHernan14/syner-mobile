import { defineStore } from "pinia";
import { computed, ref } from "vue";

import type { Usuario } from "@/data/usuario";
import { obtener_usuarios } from "@/services/usuarios_service";

export const useUsuariosStore = defineStore("usuarios", () => {
  const usuarios = ref<Usuario[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_usuarios = computed(() => usuarios.value.length > 0);

  async function cargar() {
    cargando.value = true;
    error.value = null;

    try {
      usuarios.value = await obtener_usuarios();
    } catch (err) {
      console.error("Error al cargar usuarios:", err);

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
    cargar,
  };
});
