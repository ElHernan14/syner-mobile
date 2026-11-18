<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button v-if="mostrar_volver" :default-href="ruta_volver" text="Volver" />
          <ion-menu-button v-else-if="mostrar_menu" />
        </ion-buttons>

        <ion-title>{{ titulo }}</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <ion-refresher
        v-if="mostrar_refresher"
        slot="fixed"
        @ion-refresh="actualizar"
      >
        <ion-refresher-content
          pulling-text="Desliza para actualizar"
          refreshing-spinner="circles"
        />
      </ion-refresher>

      <slot />
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import {
  IonButtons,
  IonBackButton,
  IonContent,
  IonHeader,
  IonMenuButton,
  IonPage,
  IonRefresher,
  IonRefresherContent,
  IonTitle,
  IonToolbar,
} from "@ionic/vue";

interface Props {
  titulo: string;
  mostrar_refresher?: boolean;
  mostrar_menu?: boolean;
  mostrar_volver?: boolean;
  ruta_volver?: string;
}

withDefaults(defineProps<Props>(), {
  mostrar_refresher: false,
  mostrar_menu: true,
  mostrar_volver: false,
  ruta_volver: "/app/inicio",
});

const emit = defineEmits<{
  actualizar: [];
}>();

function actualizar(event: CustomEvent) {
  emit("actualizar");

  setTimeout(() => {
    event.detail.complete();
  }, 600);
}
</script>
