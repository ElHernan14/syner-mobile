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
              <span>Plataforma</span>
            </div>
          </div>
        </ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="syner-menu">
      <ion-list :inset="false">
        <template v-for="grupo in grupos" :key="grupo.id">
          <template v-if="obtener_items_grupo(grupo.id).length > 0">
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
        </template>
      </ion-list>

      <div class="syner-menu__session">
        <div class="syner-menu__user">
          <ion-icon :icon="personCircleOutline" />

          <div class="syner-menu__user-info">
            <strong>{{ nombre_usuario }}</strong>
            <span>{{ rol_usuario }}</span>
          </div>
        </div>

        <ion-menu-toggle :auto-hide="true">
          <ion-item
            button
            lines="none"
            class="syner-menu__logout"
            @click="cerrar_sesion"
          >
            <ion-icon slot="start" :icon="logOutOutline" />

            <ion-label> Cerrar sesión </ion-label>
          </ion-item>
        </ion-menu-toggle>
      </div>
    </ion-content>
  </ion-menu>
</template>

<script setup lang="ts">
import { computed } from "vue";

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
  logOutOutline,
  peopleOutline,
  personCircleOutline,
  receiptOutline,
} from "ionicons/icons";

import { useRoute, useRouter } from "vue-router";

import { navegacion, obtener_grupos_menu } from "@/config/navegacion";

import { useAuthStore } from "@/stores/auth_store";

interface Props {
  content_id: string;
}

defineProps<Props>();

const route = useRoute();
const router = useRouter();

const auth_store = useAuthStore();

const grupos = obtener_grupos_menu();

const iconos = {
  "home-outline": homeOutline,
  "layers-outline": layersOutline,
  "receipt-outline": receiptOutline,
  "business-outline": businessOutline,
  "people-outline": peopleOutline,
};

const nombre_usuario = computed(() => {
  return auth_store.usuario?.nombre ?? "Usuario";
});

const rol_usuario = computed(() => {
  return auth_store.rol === "admin" ? "Administrador" : "Usuario";
});

function obtener_icono(nombre: string) {
  return iconos[nombre as keyof typeof iconos];
}

function tiene_acceso(item: (typeof navegacion)[number]) {
  if (!item.roles || item.roles.length === 0) {
    return true;
  }

  return auth_store.rol !== null && item.roles.includes(auth_store.rol);
}

function obtener_items_grupo(grupo_id: string) {
  return navegacion
    .filter((item) => item.grupo_menu === grupo_id && tiene_acceso(item))
    .sort((a, b) => a.orden - b.orden);
}

function es_ruta_activa(ruta: string) {
  return route.path === ruta || route.path.startsWith(`${ruta}/`);
}

async function cerrar_sesion() {
  await auth_store.cerrar_sesion();

  router.replace({
    name: "login",
  });
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

.syner-menu__session {
  margin-top: auto;
  padding: 16px;
  border-top: 1px solid var(--syner-border);
}

.syner-menu__user {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 4px 8px 12px;
}

.syner-menu__user > ion-icon {
  color: var(--syner-primary);
  font-size: 30px;
}

.syner-menu__user-info {
  min-width: 0;
}

.syner-menu__user-info strong {
  display: block;
  overflow: hidden;
  color: var(--syner-text);
  font-size: 13px;
  font-weight: 700;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.syner-menu__user-info span {
  display: block;
  margin-top: 2px;
  color: var(--syner-text-secondary);
  font-size: 11px;
}

.syner-menu__logout {
  --background: transparent;
  --background-hover: var(--syner-surface);
  --border-radius: 10px;
  --color: var(--syner-text-secondary);
}

.syner-menu__logout ion-icon {
  font-size: 20px;
}
</style>
