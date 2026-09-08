<template>
  <div v-if="total_paginas > 1" class="syner-paginacion">
    <ion-button
      fill="clear"
      :disabled="pagina <= 1"
      @click="cambiar_pagina(pagina - 1)"
    >
      Anterior
    </ion-button>

    <div class="syner-paginacion__info">
      <span>Página</span>

      <strong>{{ pagina }}</strong>

      <span>de {{ total_paginas }}</span>
    </div>

    <ion-button
      fill="clear"
      :disabled="pagina >= total_paginas"
      @click="cambiar_pagina(pagina + 1)"
    >
      Siguiente
    </ion-button>
  </div>
</template>

<script setup lang="ts">
import { IonButton } from "@ionic/vue";

defineProps<{
  pagina: number;
  total_paginas: number;
}>();

const emit = defineEmits<{
  cambiar: [pagina: number];
}>();

function cambiar_pagina(nueva_pagina: number) {
  emit("cambiar", nueva_pagina);
}
</script>

<style scoped>
.syner-paginacion {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 24px;
  padding: 8px 0;
}

.syner-paginacion__info {
  display: flex;
  align-items: center;
  gap: 5px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-paginacion__info strong {
  color: var(--syner-text);
}

.syner-paginacion ion-button {
  --padding-start: 8px;
  --padding-end: 8px;
  margin: 0;
}

@media (max-width: 576px) {
  .syner-paginacion {
    gap: 8px;
  }

  .syner-paginacion ion-button {
    font-size: 12px;
  }
}
</style>
