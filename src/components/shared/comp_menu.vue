<template>
  <ion-menu :content-id="content_id" type="overlay">
    <ion-header class="syner-menu__header">
      <ion-toolbar>
        <ion-title>
          <div class="syner-menu__brand">
            <div class="syner-menu__logo">
              <img
                :src="'/assets/icon-foreground.png'"
                :alt="'Logo de SYNER'"
              />
            </div>

            <div>
              <strong>SYNER</strong>

              <span> Plataforma </span>
            </div>
          </div>
        </ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="syner-menu">
      <ion-list :inset="false">
        <template v-for="grupo in grupos" :key="grupo.id">
          <ion-list-header class="syner-menu__group">
            {{ grupo.titulo }}
          </ion-list-header>

          <ion-menu-toggle
            v-for="item in obtener_items_grupo(grupo.id)"
            :key="item.id"
            :auto-hide="true"
          >
            <ion-item
              button
              :router-link="item.ruta"
              :class="{
                'syner-menu__item': true,
                'syner-menu__item--active': es_ruta_activa(item.ruta),
              }"
              lines="none"
            >
              <ion-icon slot="start" :icon="obtener_icono(item.icono)" />

              <ion-label>
                {{ item.titulo }}
              </ion-label>
            </ion-item>
          </ion-menu-toggle>
        </template>
      </ion-list>
    </ion-content>
  </ion-menu>
</template>

<script setup lang="ts">
import {
  IonContent,
  IonHeader,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonListHeader,
  IonMenu,
  IonMenuToggle,
  IonTitle,
  IonToolbar,
} from "@ionic/vue";

import {
  businessOutline,
  homeOutline,
  layersOutline,
  peopleOutline,
  receiptOutline,
} from "ionicons/icons";

import { navegacion, obtener_grupos_menu } from "@/config/navegacion";

import { useRoute } from "vue-router";

const route = useRoute();

interface Props {
  content_id: string;
}

defineProps<Props>();

const grupos = obtener_grupos_menu();

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

function obtener_items_grupo(grupo_id: string) {
  return navegacion
    .filter((item) => item.grupo_menu === grupo_id)
    .sort((a, b) => a.orden - b.orden);
}

function es_ruta_activa(ruta: string) {
  return route.path === ruta || route.path.startsWith(`${ruta}/`);
}
</script>

<style scoped>
.syner-menu__header ion-toolbar {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-menu__brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.syner-menu__logo {
  display: flex;
  align-items: center;
  justify-content: center;

  width: 38px;
  height: 38px;

  overflow: hidden;
  border-radius: 10px;
}

.syner-menu__logo img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.syner-menu__brand strong {
  display: block;

  color: var(--syner-text);
  font-size: 17px;
  line-height: 1;
}

.syner-menu__brand span {
  display: block;

  margin-top: 4px;

  color: var(--syner-text-secondary);
  font-size: 11px;
}

.syner-menu {
  --background: var(--syner-background);
}

.syner-menu ion-list {
  padding: 12px;
  background: transparent;
}

.syner-menu__group {
  min-height: auto;
  padding: 20px 12px 8px;

  color: var(--syner-text-secondary);

  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.syner-menu__item {
  --background: transparent;
  --background-hover: var(--syner-surface);
  --border-radius: 10px;
  --color: var(--syner-text);

  margin: 4px 0;
}

.syner-menu__item ion-icon {
  color: var(--syner-text-secondary);
  font-size: 20px;
}

.syner-menu__item ion-label {
  font-size: 14px;
}

.syner-menu__item--active {
  --background: var(--syner-primary);
  --color: #ffffff;
}

.syner-menu__item--active ion-icon {
  color: #ffffff;
}
</style>
