import { ref } from "vue";

import { obtener_health } from "@/services/health_service";

export function useHealth() {
  const estado = ref<string | null>(null);
  const mensaje = ref<string | null>(null);
  const cargando = ref(false);
  const error = ref<string | null>(null);

  async function probar_conexion() {
    cargando.value = true;
    estado.value = null;
    mensaje.value = null;
    error.value = null;

    try {
      const respuesta = await obtener_health();

      estado.value = respuesta.estado;
      mensaje.value = respuesta.mensaje;
    } catch (err) {
      console.error("Error al comprobar la conexión:", err);

      error.value =
        err instanceof Error ? err.message : "No se pudo conectar con la API.";
    } finally {
      cargando.value = false;
    }
  }

  return {
    estado,
    mensaje,
    cargando,
    error,
    probar_conexion,
  };
}
