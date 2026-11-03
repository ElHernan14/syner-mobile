<template>
  <section class="syner-estado-listado" :class="`syner-estado-listado--${tipo}`" :role="tipo === 'error' ? 'alert' : 'status'">
    <ion-icon :icon="tipo === 'error' ? alertCircleOutline : fileTrayOutline" />
    <h2>{{ titulo }}</h2>
    <p>{{ mensaje }}</p>
    <ion-button v-if="reintentar" fill="outline" @click="$emit('reintentar')">
      Reintentar
    </ion-button>
  </section>
</template>

<script setup lang="ts">
import { IonButton, IonIcon } from "@ionic/vue";
import { alertCircleOutline, fileTrayOutline } from "ionicons/icons";

withDefaults(defineProps<{
  tipo: "error" | "vacio";
  titulo?: string;
  mensaje: string;
  reintentar?: boolean;
}>(), {
  titulo: undefined,
  reintentar: false,
});

defineEmits<{ reintentar: [] }>();

</script>

<style scoped>
.syner-estado-listado { display: flex; min-height: 260px; flex-direction: column; align-items: center; justify-content: center; gap: 12px; padding: 32px 24px; color: var(--syner-text-secondary); text-align: center; }
.syner-estado-listado ion-icon { color: var(--syner-primary); font-size: 46px; }
.syner-estado-listado--error ion-icon { color: var(--ion-color-danger); }
.syner-estado-listado h2, .syner-estado-listado p { margin: 0; }
.syner-estado-listado h2 { color: var(--syner-text); font-size: 20px; }
</style>
