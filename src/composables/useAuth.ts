import { storeToRefs } from "pinia";

import { useAuthStore } from "@/stores/auth_store";

export function useAuth() {
  const auth_store = useAuthStore();

  const {
    access_token,
    refresh_token,
    usuario,
    cargando,
    error,
    autenticado,
    rol,
    es_admin,
    es_usuario,
  } = storeToRefs(auth_store);

  function iniciar_sesion(
    request: Parameters<typeof auth_store.iniciar_sesion>[0],
  ) {
    return auth_store.iniciar_sesion(request);
  }

  function cerrar_sesion() {
    auth_store.cerrar_sesion();
  }

  return {
    access_token,
    refresh_token,
    usuario,
    cargando,
    error,
    autenticado,
    rol,
    es_admin,
    es_usuario,
    iniciar_sesion,
    cerrar_sesion,
  };
}
