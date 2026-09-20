import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { LoginRequest } from "@/models/auth";

import type { Usuario } from "@/models/usuario";

import { iniciar_sesion, refrescar_sesion } from "@/services/auth_service";

import {
  biometria_disponible,
  eliminar_sesion_biometrica,
  existe_sesion_biometrica,
  guardar_refresh_token_biometria,
  obtener_refresh_token_biometria,
} from "@/services/biometria_service";

const STORAGE_KEY = "syner_auth_session";

interface SesionPersistida {
  accessToken: string;
  refreshToken: string;
  usuario: Usuario;
}

function obtener_sesion_persistida(): SesionPersistida | null {
  const datos = localStorage.getItem(STORAGE_KEY);

  if (!datos) {
    return null;
  }

  try {
    const sesion = JSON.parse(datos) as SesionPersistida;

    if (
      typeof sesion.accessToken !== "string" ||
      typeof sesion.refreshToken !== "string" ||
      !sesion.usuario
    ) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return sesion;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function guardar_sesion(
  access_token: string,
  refresh_token: string,
  usuario: Usuario,
  usar_biometria: boolean,
) {
  if (usar_biometria) {
    localStorage.removeItem(STORAGE_KEY);
    return;
  }

  const sesion: SesionPersistida = {
    accessToken: access_token,
    refreshToken: refresh_token,
    usuario,
  };

  localStorage.setItem(STORAGE_KEY, JSON.stringify(sesion));
}

function eliminar_sesion_persistida() {
  localStorage.removeItem(STORAGE_KEY);
}

function obtener_mensaje_error(
  error: unknown,
  mensaje_generico: string,
): string {
  if (error instanceof Error && error.message.trim()) {
    return error.message;
  }

  return mensaje_generico;
}

export const useAuthStore = defineStore("auth", () => {
  const sesion_persistida = obtener_sesion_persistida();

  const access_token = ref<string | null>(
    sesion_persistida?.accessToken ?? null,
  );

  const refresh_token = ref<string | null>(
    sesion_persistida?.refreshToken ?? null,
  );

  const usuario = ref<Usuario | null>(sesion_persistida?.usuario ?? null);

  const cargando = ref(false);

  const error = ref<string | null>(null);

  const biometria_activa = ref(false);

  const autenticado = computed(() => {
    return access_token.value !== null && usuario.value !== null;
  });

  const rol = computed(() => {
    return usuario.value?.rol ?? null;
  });

  const es_admin = computed(() => {
    return rol.value === "admin";
  });

  const es_usuario = computed(() => {
    return rol.value === "usuario";
  });

  async function iniciar_sesion_store(request: LoginRequest) {
    cargando.value = true;
    error.value = null;

    try {
      const respuesta = await iniciar_sesion(request);

      if (!respuesta.exito || !respuesta.datos) {
        const mensaje = respuesta.mensaje || "No se pudo iniciar sesión.";

        error.value = mensaje;

        throw new Error(mensaje);
      }

      const datos = respuesta.datos;

      access_token.value = datos.accessToken;
      refresh_token.value = datos.refreshToken;
      usuario.value = datos.usuario;

      guardar_sesion(
        datos.accessToken,
        datos.refreshToken,
        datos.usuario,
        biometria_activa.value,
      );

      return respuesta;
    } catch (err) {
      access_token.value = null;
      refresh_token.value = null;
      usuario.value = null;

      eliminar_sesion_persistida();

      const mensaje = obtener_mensaje_error(err, "No se pudo iniciar sesión.");

      error.value = mensaje;

      throw new Error(mensaje);
    } finally {
      cargando.value = false;
    }
  }

  async function refrescar_sesion_store() {
    if (!refresh_token.value) {
      throw new Error("No existe un refresh token.");
    }

    try {
      const respuesta = await refrescar_sesion({
        refreshToken: refresh_token.value,
      });

      if (!respuesta.exito || !respuesta.datos) {
        const mensaje = respuesta.mensaje || "No se pudo refrescar la sesión.";

        throw new Error(mensaje);
      }

      const datos = respuesta.datos;

      access_token.value = datos.accessToken;
      refresh_token.value = datos.refreshToken;
      usuario.value = datos.usuario;

      guardar_sesion(
        datos.accessToken,
        datos.refreshToken,
        datos.usuario,
        biometria_activa.value,
      );

      return respuesta;
    } catch (err) {
      access_token.value = null;
      refresh_token.value = null;
      usuario.value = null;

      eliminar_sesion_persistida();

      const mensaje = obtener_mensaje_error(err, "La sesión expiró.");

      error.value = mensaje;

      throw new Error(mensaje);
    }
  }

  async function activar_biometria(): Promise<boolean> {
    if (!refresh_token.value) {
      return false;
    }

    const disponible = await biometria_disponible();

    if (!disponible) {
      return false;
    }

    try {
      await guardar_refresh_token_biometria(refresh_token.value);

      biometria_activa.value = true;

      // El refresh token deja de estar en localStorage.
      eliminar_sesion_persistida();

      return true;
    } catch (err) {
      console.error("No se pudo activar la biometría:", err);

      return false;
    }
  }

  async function desactivar_biometria(): Promise<void> {
    await eliminar_sesion_biometrica();

    biometria_activa.value = false;

    if (access_token.value && refresh_token.value && usuario.value) {
      guardar_sesion(
        access_token.value,
        refresh_token.value,
        usuario.value,
        false,
      );
    }
  }

  async function iniciar_con_biometria(): Promise<boolean> {
    if (autenticado.value) {
      return true;
    }

    const existe = await existe_sesion_biometrica();

    if (!existe) {
      return false;
    }

    const refresh_token_biometrico = await obtener_refresh_token_biometria();

    if (!refresh_token_biometrico) {
      return false;
    }

    try {
      biometria_activa.value = true;

      refresh_token.value = refresh_token_biometrico;

      await refrescar_sesion_store();

      return true;
    } catch (err) {
      console.error("No se pudo recuperar la sesión mediante biometría:", err);

      access_token.value = null;
      refresh_token.value = null;
      usuario.value = null;

      biometria_activa.value = false;

      eliminar_sesion_persistida();

      return false;
    }
  }

  async function cerrar_sesion() {
    await eliminar_sesion_biometrica();

    access_token.value = null;
    refresh_token.value = null;
    usuario.value = null;

    biometria_activa.value = false;

    eliminar_sesion_persistida();
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
    biometria_activa,
    iniciar_sesion: iniciar_sesion_store,
    refrescar_sesion: refrescar_sesion_store,
    cerrar_sesion,
    activar_biometria,
    desactivar_biometria,
    iniciar_con_biometria,
  };
});
