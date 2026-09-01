import { storeToRefs } from "pinia";

import { useUsuariosStore } from "@/stores/usuarios_store";

export function useUsuarios() {
  const usuarios_store = useUsuariosStore();

  const { usuarios, cargando, error, hay_usuarios } =
    storeToRefs(usuarios_store);

  function cargar_usuarios() {
    return usuarios_store.cargar();
  }

  return {
    usuarios,
    cargando,
    error,
    hay_usuarios,
    cargar_usuarios,
  };
}
