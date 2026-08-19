<template>
  <ion-page>
    <comp-menu :content_id="tabs_id" />

    <ion-tabs :id="tabs_id">
      <ion-router-outlet />

      <comp-tabs />
    </ion-tabs>

    <div class="syner-theme-toggle">
      <ThemeToggle />
    </div>
  </ion-page>
</template>

<script setup lang="ts">
import { IonPage, IonTabs, IonRouterOutlet } from "@ionic/vue";

import CompMenu from "@/components/shared/comp_menu.vue";
import CompTabs from "@/components/shared/comp_tabs.vue";
import ThemeToggle from "@/components/theme/ThemeToggle.vue";

const tabs_id = "syner-main-tabs";
</script>

<style scoped>
.syner-theme-toggle {
  position: fixed;
  top: 8px;
  right: 8px;
  z-index: 100;
}
</style>
