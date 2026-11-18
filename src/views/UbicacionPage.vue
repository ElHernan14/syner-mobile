<template>
  <comp-page titulo="Mi ubicación">
    <div class="ion-padding syner-ubicacion">
      <div class="flex min-h-full flex-col justify-center gap-6">
        <div class="text-center">
          <div
            class="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-full bg-primary/10"
          >
            <ion-icon :icon="location" class="text-5xl text-primary" />
          </div>

          <h1 class="text-2xl font-bold">Ubicación actual</h1>

          <p class="mt-2 text-sm syner-ubicacion__muted">
            Obtené la ubicación actual de tu dispositivo.
          </p>
        </div>

        <div v-if="ubicacion" class="syner-ubicacion__data rounded-xl border p-5">
          <div class="space-y-4">
            <div>
              <p class="text-sm syner-ubicacion__muted">Latitud</p>

              <p class="syner-ubicacion__value font-mono text-lg font-semibold">
                {{ ubicacion.latitud }}
              </p>
            </div>

            <div>
              <p class="text-sm syner-ubicacion__muted">Longitud</p>

              <p class="syner-ubicacion__value font-mono text-lg font-semibold">
                {{ ubicacion.longitud }}
              </p>
            </div>

            <div v-if="ubicacion.precision !== null">
              <p class="text-sm syner-ubicacion__muted">Precisión</p>

              <p class="syner-ubicacion__value font-mono text-lg font-semibold">
                {{ ubicacion.precision }} m
              </p>
            </div>
          </div>
        </div>

        <ion-button
          expand="block"
          :disabled="cargando"
          @click="obtener_ubicacion"
        >
          <ion-spinner v-if="cargando" name="crescent" class="mr-2" />

          {{ cargando ? "Obteniendo ubicación..." : "Obtener ubicación" }}
        </ion-button>

        <ion-text v-if="error" color="danger" class="text-center">
          {{ error }}
        </ion-text>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref } from "vue";

import {
  IonButton,
  IonIcon,
  IonSpinner,
  IonText,
} from "@ionic/vue";
import CompPage from "@/components/shared/comp_page.vue";

import { location } from "ionicons/icons";

import { obtener_ubicacion_actual } from "@/services/geolocalizacion_service";

interface UbicacionActual {
  latitud: number;
  longitud: number;
  precision: number | null;
}

const ubicacion = ref<UbicacionActual | null>(null);
const cargando = ref(false);
const error = ref<string | null>(null);

async function obtener_ubicacion() {
  cargando.value = true;
  error.value = null;

  try {
    const posicion = await obtener_ubicacion_actual();

    ubicacion.value = {
      latitud: posicion.coords.latitude,
      longitud: posicion.coords.longitude,
      precision: posicion.coords.accuracy ?? null,
    };
  } catch (err) {
    error.value =
      err instanceof Error ? err.message : "No se pudo obtener la ubicación.";
  } finally {
    cargando.value = false;
  }
}
</script>

<style scoped>
.syner-ubicacion { color: var(--syner-text); }
.syner-ubicacion__muted { color: var(--syner-text-secondary); }
.syner-ubicacion__data { border-color: var(--syner-border); background: var(--syner-surface); }
.syner-ubicacion__value { color: var(--syner-text); }
</style>
