<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-menu-button />
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
}

withDefaults(defineProps<Props>(), {
  mostrar_refresher: false,
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
