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

  function iniciar_sesion_google() {
    return auth_store.iniciar_sesion_google();
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
    iniciar_sesion_google,
    cerrar_sesion,
  };
}
