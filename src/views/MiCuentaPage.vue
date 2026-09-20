<template>
  <comp-page :titulo="'Mi Cuenta'">
    <div class="syner-mi-cuenta">
      <ion-list :inset="true">
        <ion-item>
          <ion-icon slot="start" :icon="pulseOutline" />

          <ion-label>
            <h2>Diagnóstico</h2>

            <p>Esta app consulta los datos desde:</p>

            <p class="syner-mi-cuenta__url">
              {{ api_url }}
            </p>
          </ion-label>
        </ion-item>
      </ion-list>

      <div class="syner-mi-cuenta__accion">
        <ion-button
          expand="block"
          :disabled="probando"
          @click="probar_conexion"
        >
          {{ probando ? "Probando..." : "PROBAR CONEXIÓN" }}
        </ion-button>
      </div>

      <ion-list :inset="true" class="syner-mi-cuenta__seguridad">
        <ion-list-header>
          <ion-label>Seguridad</ion-label>
        </ion-list-header>

        <ion-item>
          <ion-icon slot="start" :icon="fingerPrintOutline" />

          <ion-label>
            <h2>Acceso con huella</h2>

            <p v-if="cargando_biometria">Verificando disponibilidad...</p>

            <p v-else-if="!biometria_disponible_local">
              La biometría no está disponible en este dispositivo.
            </p>

            <p v-else-if="auth_store.biometria_activa">
              La huella está activada para iniciar sesión.
            </p>

            <p v-else>
              Usá la huella del dispositivo para iniciar sesión de forma segura.
            </p>
          </ion-label>

          <ion-toggle
            slot="end"
            :checked="auth_store.biometria_activa"
            :disabled="
              cargando_biometria ||
              !biometria_disponible_local ||
              procesando_biometria
            "
            @ion-change="cambiar_biometria"
          />
        </ion-item>
      </ion-list>

      <div
        v-if="mensaje_biometria"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': mensaje_biometria.estado === 'ok',

          'syner-mi-cuenta__resultado--error':
            mensaje_biometria.estado === 'error',
        }"
      >
        {{ mensaje_biometria.mensaje }}
      </div>

      <div
        v-if="resultado"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': resultado.estado === 'ok',

          'syner-mi-cuenta__resultado--error': resultado.estado === 'error',
        }"
      >
        {{ resultado.mensaje }}
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

import {
  IonButton,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonListHeader,
  IonToggle,
} from "@ionic/vue";

import { fingerPrintOutline, pulseOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";

import { obtener } from "@/services/ajax_service";

import { biometria_disponible } from "@/services/biometria_service";

import { obtener_api_url } from "@/config/debug";

import { useAuthStore } from "@/stores/auth_store";

interface HealthResponse {
  estado: string;
  mensaje: string;
}

const auth_store = useAuthStore();

const api_url = obtener_api_url();

const probando = ref(false);

const cargando_biometria = ref(true);

const procesando_biometria = ref(false);

const biometria_disponible_local = ref(false);

const mensaje_biometria = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

const resultado = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

async function cargar_disponibilidad_biometria() {
  cargando_biometria.value = true;

  try {
    biometria_disponible_local.value = await biometria_disponible();
  } catch {
    biometria_disponible_local.value = false;
  } finally {
    cargando_biometria.value = false;
  }
}

async function cambiar_biometria(event: CustomEvent) {
  const activar = event.detail.checked;

  mensaje_biometria.value = null;

  procesando_biometria.value = true;

  try {
    if (activar) {
      const activada = await auth_store.activar_biometria();

      if (!activada) {
        mensaje_biometria.value = {
          estado: "error",
          mensaje: "No se pudo activar el acceso con huella.",
        };

        return;
      }

      mensaje_biometria.value = {
        estado: "ok",
        mensaje: "Acceso con huella activado correctamente.",
      };

      return;
    }

    await auth_store.desactivar_biometria();

    mensaje_biometria.value = {
      estado: "ok",
      mensaje: "Acceso con huella desactivado.",
    };
  } finally {
    procesando_biometria.value = false;
  }
}

async function probar_conexion() {
  probando.value = true;

  resultado.value = null;

  try {
    const respuesta = await obtener<HealthResponse>("health");

    resultado.value = {
      estado: "ok",
      mensaje: respuesta.mensaje,
    };
  } catch (err) {
    console.error("Error al probar conexión:", err);

    resultado.value = {
      estado: "error",
      mensaje: "No se pudo establecer conexión con la API.",
    };
  } finally {
    probando.value = false;
  }
}

onMounted(async () => {
  await cargar_disponibilidad_biometria();
});
</script>

<style scoped>
.syner-mi-cuenta {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-mi-cuenta ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-mi-cuenta ion-icon {
  color: var(--syner-primary);
}

.syner-mi-cuenta__url {
  margin-top: 8px;
  word-break: break-all;
  color: var(--syner-text);
  font-family: monospace;
  font-size: 13px;
}

.syner-mi-cuenta__accion {
  margin-top: 24px;
}

.syner-mi-cuenta__seguridad {
  margin-top: 24px;
}

.syner-mi-cuenta__resultado {
  margin-top: 16px;
  padding: 14px;
  border-radius: 8px;
  text-align: center;
  font-size: 14px;
}

.syner-mi-cuenta__resultado--ok {
  color: var(--syner-success);
}

.syner-mi-cuenta__resultado--error {
  color: #ef4444;
}
</style>
