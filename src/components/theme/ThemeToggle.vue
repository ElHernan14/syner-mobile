<template>
  <ion-button
    fill="clear"
    :aria-label="isDark ? 'Activar tema claro' : 'Activar tema oscuro'"
    @click="themeStore.toggleTheme"
  >
    <ion-icon slot="icon-only" :icon="isDark ? sunnyOutline : moonOutline" />
  </ion-button>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { IonButton, IonIcon } from "@ionic/vue";
import { moonOutline, sunnyOutline } from "ionicons/icons";
import { useThemeStore } from "@/stores/theme.store";

const themeStore = useThemeStore();

const isDark = computed(() => themeStore.isDark);
</script>
