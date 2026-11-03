<template>
  <ion-badge :color="visual.color" class="syner-estado-badge">
    <ion-icon :icon="visual.icono" aria-hidden="true" />
    <span>{{ visual.etiqueta }}</span>
  </ion-badge>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { IonBadge, IonIcon } from "@ionic/vue";
import { obtener_estado_visual } from "@/config/estados";

const props = defineProps<{ estado: string }>();
const visual = computed(() => obtener_estado_visual(props.estado));
</script>

<style scoped>
.syner-estado-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  vertical-align: middle;
}

.syner-estado-badge ion-icon { font-size: 14px; }
</style>
