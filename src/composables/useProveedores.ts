import { storeToRefs } from "pinia";

import { useProveedoresStore } from "@/stores/proveedores_store";

export function useProveedores() {
  const proveedores_store = useProveedoresStore();

  const { proveedores, cargando, error, hay_proveedores } =
    storeToRefs(proveedores_store);

  function cargar_proveedores() {
    return proveedores_store.cargar();
  }

  return {
    proveedores,
    cargando,
    error,
    hay_proveedores,
    cargar_proveedores,
  };
}
