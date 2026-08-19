<template>
  <ion-tab-bar slot="bottom" class="syner-tabs">
    <ion-tab-button
      v-for="item in obtener_tabs()"
      :key="item.id"
      :tab="item.id"
      :href="item.ruta"
      class="syner-tabs__item"
      :selected="es_tab_activo(item.ruta)"
    >
      <ion-icon :icon="obtener_icono(item.icono)" />

      <ion-label>
        {{ item.titulo }}
      </ion-label>
    </ion-tab-button>
  </ion-tab-bar>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

import { IonIcon, IonLabel, IonTabBar, IonTabButton } from "@ionic/vue";

import {
  businessOutline,
  homeOutline,
  layersOutline,
  peopleOutline,
  receiptOutline,
} from "ionicons/icons";

import { obtener_tabs } from "@/config/navegacion";

const route = useRoute();

const iconos = {
  "home-outline": homeOutline,
  "layers-outline": layersOutline,
  "receipt-outline": receiptOutline,
  "business-outline": businessOutline,
  "people-outline": peopleOutline,
};

function obtener_icono(nombre: string) {
  return iconos[nombre as keyof typeof iconos];
}

const ruta_actual = computed(() => route.path);

function es_tab_activo(ruta: string) {
  return ruta_actual.value === ruta;
}
</script>

<style scoped>
.syner-tabs {
  --background: var(--syner-surface);
  --border: var(--syner-border);

  height: 64px;

  border-top: 1px solid var(--syner-border);
}

.syner-tabs__item {
  --color: var(--syner-text-secondary);
  --color-selected: var(--syner-primary);

  max-width: 120px;

  font-size: 11px;
  font-weight: 500;
}

.syner-tabs__item ion-icon {
  margin-bottom: 3px;

  font-size: 21px;

  transition: transform 0.2s ease, opacity 0.2s ease;
}

.syner-tabs__item ion-label {
  font-size: 11px;
}

.syner-tabs__item.tab-selected {
  font-weight: 700;
}

.syner-tabs__item.tab-selected ion-icon {
  transform: scale(1.08);
}
</style>
