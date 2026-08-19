<template>
  <ion-list>
    <ion-item v-for="fila in filas" :key="fila">
      <ion-label>
        <ion-skeleton-text :animated="true" style="width: 70%" />

        <ion-skeleton-text :animated="true" style="width: 45%" />

        <ion-skeleton-text :animated="true" style="width: 85%" />
      </ion-label>
    </ion-item>
  </ion-list>
</template>

<script setup lang="ts">
import { IonItem, IonLabel, IonList, IonSkeletonText } from "@ionic/vue";

interface Props {
  filas?: number;
}

withDefaults(defineProps<Props>(), {
  filas: 5,
});
</script>
