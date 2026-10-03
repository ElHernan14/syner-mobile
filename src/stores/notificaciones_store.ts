import { computed, ref } from "vue";
import { defineStore } from "pinia";

import type { Notificacion } from "@/models/notificacion";
import {
  marcar_como_leida,
  obtener_no_leidas,
  obtener_notificaciones,
} from "@/services/notificaciones_service";

export const useNotificacionesStore = defineStore("notificaciones", () => {
  const notificaciones = ref<Notificacion[]>([]);
  const cantidad_no_leidas = ref(0);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  const hay_no_leidas = computed(() => {
    return cantidad_no_leidas.value > 0;
  });

  async function cargar_notificaciones() {
    cargando.value = true;
    error.value = null;

    try {
      const respuesta = await obtener_notificaciones();

      if (!respuesta.exito || !respuesta.datos) {
        throw new Error(
          respuesta.mensaje || "No se pudieron obtener las notificaciones.",
        );
      }

      notificaciones.value = respuesta.datos;

      cantidad_no_leidas.value = respuesta.datos.filter(
        (notificacion) => !notificacion.leida,
      ).length;
    } catch (exception) {
      error.value =
        exception instanceof Error
          ? exception.message
          : "No se pudieron obtener las notificaciones.";

      throw exception;
    } finally {
      cargando.value = false;
    }
  }

  async function cargar_no_leidas() {
    try {
      const respuesta = await obtener_no_leidas();

      if (!respuesta.exito || !respuesta.datos) {
        throw new Error(
          respuesta.mensaje ||
            "No se pudieron obtener las notificaciones no leídas.",
        );
      }

      cantidad_no_leidas.value = respuesta.datos.length;

      return respuesta.datos;
    } catch (exception) {
      error.value =
        exception instanceof Error
          ? exception.message
          : "No se pudieron obtener las notificaciones no leídas.";

      throw exception;
    }
  }

  async function marcar_leida(id: number) {
    const notificacion = notificaciones.value.find((item) => item.id === id);

    const respuesta = await marcar_como_leida(id);

    if (!respuesta.exito || !respuesta.datos) {
      throw new Error(
        respuesta.mensaje || "No se pudo marcar la notificación como leída.",
      );
    }

    if (notificacion && !notificacion.leida) {
      notificacion.leida = true;
    }

    if (cantidad_no_leidas.value > 0) {
      cantidad_no_leidas.value--;
    }
  }

  function limpiar() {
    notificaciones.value = [];
    cantidad_no_leidas.value = 0;
    error.value = null;
  }

  return {
    notificaciones,
    cantidad_no_leidas,
    hay_no_leidas,
    cargando,
    error,
    cargar_notificaciones,
    cargar_no_leidas,
    marcar_leida,
    limpiar,
  };
});
