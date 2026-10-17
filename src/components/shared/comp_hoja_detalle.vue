<template>
  <ion-modal
    :is-open="abierta"
    :initial-breakpoint="0.9"
    :breakpoints="[0, 0.5, 0.9]"
    @did-dismiss="$emit('cerrar')"
  >
    <ion-header>
      <ion-toolbar>
        <ion-title>{{ titulo }}</ion-title>
        <ion-buttons slot="end">
          <ion-button aria-label="Cerrar detalle" @click="$emit('cerrar')">
            <ion-icon slot="icon-only" :icon="closeOutline" />
          </ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <slot />
    </ion-content>
  </ion-modal>
</template>

<script setup lang="ts">
import {
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonIcon,
  IonModal,
  IonTitle,
  IonToolbar,
} from "@ionic/vue";
import { closeOutline } from "ionicons/icons";

defineProps<{
  abierta: boolean;
  titulo: string;
}>();

defineEmits<{ cerrar: [] }>();
</script>
