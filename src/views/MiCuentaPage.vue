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
import { ref } from "vue";
import { IonButton, IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";
import { pulseOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import { obtener } from "@/services/ajax_service";
import { obtener_api_url } from "@/config/debug";

interface HealthResponse {
  estado: string;
  mensaje: string;
}

const api_url = obtener_api_url();

const probando = ref(false);

const resultado = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

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
