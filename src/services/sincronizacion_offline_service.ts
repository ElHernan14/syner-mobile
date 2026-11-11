import type { Pinia } from "pinia";
import { useAuthStore } from "@/stores/auth_store";
import { useLotesStore } from "@/stores/lotes_store";
import { sincronizar_cola_lotes } from "@/services/cola_offline_lotes_service";
import {
  escuchar_cambios_conexion,
  hay_conexion,
} from "@/services/conectividad_service";

let iniciado = false;
let sincronizacion_en_curso = false;

export function iniciar_sincronizacion_offline(pinia: Pinia): void {
  if (iniciado) return;

  iniciado = true;

  async function procesar_cola(): Promise<void> {
    if (!(await hay_conexion()) || sincronizacion_en_curso) return;

    const auth_store = useAuthStore(pinia);
    const usuario_id = auth_store.usuario?.id;

    // Solo sincronizamos las operaciones del administrador autenticado.
    if (!auth_store.es_admin || !usuario_id) return;

    sincronizacion_en_curso = true;

    try {
      const lotes_store = useLotesStore(pinia);

      const operaciones_antes = lotes_store.operaciones_pendientes.length;

      await sincronizar_cola_lotes(usuario_id);

      lotes_store.refrescar_operaciones_pendientes();

      const operaciones_despues = lotes_store.operaciones_pendientes.length;

      if (
        (await hay_conexion()) &&
        operaciones_despues < operaciones_antes
      ) {
        await lotes_store.cargar(lotes_store.page);
      }
    } catch (error) {
      console.error(
        "No se pudo completar la sincronización offline de lotes:",
        error,
      );
    } finally {
      sincronizacion_en_curso = false;
    }
  }

  void escuchar_cambios_conexion((conectado) => {
    if (conectado) void procesar_cola();
  }).catch((error) => {
    console.error("No se pudo escuchar el estado de red:", error);
  });

  // La cola puede cambiar mientras la app está abierta.
  window.addEventListener("syner:cola-lotes-actualizada", () => {
    const lotes_store = useLotesStore(pinia);
    lotes_store.refrescar_operaciones_pendientes();
  });

  // También intentamos sincronizar al iniciar la app.
  void procesar_cola();
}
