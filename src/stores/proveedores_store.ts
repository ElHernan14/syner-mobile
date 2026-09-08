import { defineStore } from "pinia";
import { computed, ref } from "vue";

import type { Proveedor } from "@/models/proveedor";
import { obtener_proveedores } from "@/services/proveedores_service";

export const useProveedoresStore = defineStore("proveedores", () => {
  const proveedores = ref<Proveedor[]>([]);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_proveedores = computed(() => proveedores.value.length > 0);

  async function cargar() {
    cargando.value = true;
    error.value = null;

    try {
      proveedores.value = await obtener_proveedores();
    } catch (err) {
      console.error("Error al cargar proveedores:", err);

      error.value =
        err instanceof Error
          ? err.message
          : "No se pudieron cargar los proveedores.";
    } finally {
      cargando.value = false;
    }
  }

  return {
    proveedores,
    cargando,
    error,
    hay_proveedores,
    cargar,
  };
});
