import { computed, ref } from "vue";

import { defineStore } from "pinia";

import type { LoginRequest } from "@/models/auth";

import type { Usuario } from "@/models/usuario";

import { iniciar_sesion, refrescar_sesion } from "@/services/auth_service";
import { es_error_conexion } from "@/services/ajax_service";

import {
  biometria_disponible,
  eliminar_sesion_biometrica,
  existe_sesion_biometrica,
  guardar_refresh_token_biometria,
  obtener_refresh_token_biometria,
} from "@/services/biometria_service";

const STORAGE_KEY = "syner_auth_session";
const BIOMETRIC_PROFILE_KEY = "syner_biometric_profile";

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

    return {
      ...sesion,
      usuario: { ...sesion.usuario, id: String(sesion.usuario.id) },
    };
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
    localStorage.setItem(BIOMETRIC_PROFILE_KEY, JSON.stringify(usuario));
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

function obtener_status_error(error: unknown): number | null {
  if (typeof error !== "object" || error === null || !("status" in error)) {
    return null;
  }

  const status = error.status;

  return typeof status === "number" ? status : null;
}

function es_error_transitorio(error: unknown): boolean {
  if (
    error instanceof Error &&
    es_error_conexion(error)
  ) {
    return true;
  }

  const status = obtener_status_error(error);

  return status === 0 || (status !== null && status >= 500);
}

async function registrar_push_despues_autenticacion(): Promise<void> {
  try {
    const { registrar_push } = await import(
      "@/services/push_notifications_service"
    );

    await registrar_push();
  } catch (error) {
    console.error(
      "No se pudo registrar el dispositivo para Push Notifications:",
      error,
    );
  }
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
  const sesion_offline = ref(false);

  const autenticado = computed(() => {
    return usuario.value !== null && (access_token.value !== null || sesion_offline.value);
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
      sesion_offline.value = false;
      refresh_token.value = datos.refreshToken;
      const usuario_normalizado = {
        ...datos.usuario,
        id: String(datos.usuario.id),
      };
      usuario.value = usuario_normalizado;

      guardar_sesion(
        datos.accessToken,
        datos.refreshToken,
        usuario_normalizado,
        biometria_activa.value,
      );

      await registrar_push_despues_autenticacion();

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
      sesion_offline.value = false;
      refresh_token.value = datos.refreshToken;
      const usuario_normalizado = {
        ...datos.usuario,
        id: String(datos.usuario.id),
      };
      usuario.value = usuario_normalizado;

      guardar_sesion(
        datos.accessToken,
        datos.refreshToken,
        usuario_normalizado,
        biometria_activa.value,
      );

      return respuesta;
    } catch (err) {
      const mensaje = obtener_mensaje_error(
        err,
        "No se pudo refrescar la sesión.",
      );

      error.value = mensaje;

      if (es_error_transitorio(err)) {
        // Conservamos la sesión y el tipo original del error.
        throw err;
      }

      access_token.value = null;
      refresh_token.value = null;
      usuario.value = null;
      sesion_offline.value = false;

      eliminar_sesion_persistida();

      throw err;
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

      if (usuario.value) localStorage.setItem(BIOMETRIC_PROFILE_KEY, JSON.stringify(usuario.value));

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
    localStorage.removeItem(BIOMETRIC_PROFILE_KEY);

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

      await registrar_push_despues_autenticacion();

      return true;
    } catch (err) {
      console.error("No se pudo recuperar la sesión mediante biometría:", err);

      if (es_error_conexion(err)) {
        try {
          const perfil = JSON.parse(localStorage.getItem(BIOMETRIC_PROFILE_KEY) ?? "null") as Usuario | null;
          if (perfil) {
            usuario.value = { ...perfil, id: String(perfil.id) };
            access_token.value = null;
            refresh_token.value = refresh_token_biometrico;
            biometria_activa.value = true;
            sesion_offline.value = true;
            return true;
          }
        } catch { /* Se continúa a la pantalla de ingreso si el perfil local no es válido. */ }
      }

      access_token.value = null;
      refresh_token.value = null;
      usuario.value = null;
      biometria_activa.value = false;
      sesion_offline.value = false;

      eliminar_sesion_persistida();
      await eliminar_sesion_biometrica();
      localStorage.removeItem(BIOMETRIC_PROFILE_KEY);

      return false;
    }
  }

  async function cerrar_sesion() {
    try {
      const { desactivar_push } = await import(
        "@/services/push_notifications_service"
      );

      await desactivar_push();
    } catch (error) {
      console.error("No se pudo desactivar el dispositivo Push:", error);
    }

    await eliminar_sesion_biometrica();
    localStorage.removeItem(BIOMETRIC_PROFILE_KEY);

    access_token.value = null;
    refresh_token.value = null;
    usuario.value = null;
    biometria_activa.value = false;
    sesion_offline.value = false;

    eliminar_sesion_persistida();
  }

  function actualizar_avatar_local(avatar: string) {
    if (!usuario.value) return;
    usuario.value.avatar = avatar;
    const sesion = obtener_sesion_persistida();
    if (sesion && String(sesion.usuario.id) === String(usuario.value.id)) {
      guardar_sesion(sesion.accessToken, sesion.refreshToken, usuario.value, biometria_activa.value);
    }
    if (biometria_activa.value) localStorage.setItem(BIOMETRIC_PROFILE_KEY, JSON.stringify(usuario.value));
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
    sesion_offline,
    iniciar_sesion: iniciar_sesion_store,
    refrescar_sesion: refrescar_sesion_store,
    cerrar_sesion,
    actualizar_avatar_local,
    activar_biometria,
    desactivar_biometria,
    iniciar_con_biometria,
  };
});
