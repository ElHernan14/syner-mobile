<template>
  <div class="syner-lote-filtros">
    <div class="syner-lote-filtros__search">
      <ion-searchbar
        :value="termino"
        placeholder="Buscar lotes..."
        :debounce="0"
        :clear-input="false"
        @ion-input="manejar_busqueda"
      />

      <ion-button v-if="termino" fill="clear" size="small" @click="limpiar">
        Limpiar
      </ion-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { IonButton, IonSearchbar } from "@ionic/vue";

defineProps<{
  termino: string;
}>();

const emit = defineEmits<{
  buscar: [termino: string];
}>();

function manejar_busqueda(event: CustomEvent) {
  const valor = event.detail.value ?? "";

  emit("buscar", valor);
}

function limpiar() {
  emit("buscar", "");
}
</script>

<style scoped>
.syner-lote-filtros {
  margin-bottom: 20px;
}

.syner-lote-filtros__search {
  display: flex;
  align-items: center;
  gap: 8px;
}

.syner-lote-filtros ion-searchbar {
  --background: var(--syner-surface);
  --color: var(--syner-text);
  --placeholder-color: var(--syner-text-secondary);
  --icon-color: var(--syner-text-secondary);
  --border-radius: 12px;

  flex: 1;
  padding: 0;
}

.syner-lote-filtros ion-button {
  flex-shrink: 0;
}
</style>
