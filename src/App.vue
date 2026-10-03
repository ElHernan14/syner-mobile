<template>
  <ion-app>
    <ion-router-outlet />
  </ion-app>
</template>

<script setup lang="ts">
import { onMounted } from "vue";

import { IonApp, IonRouterOutlet } from "@ionic/vue";

import { useRouter } from "vue-router";

import { useAuthStore } from "@/stores/auth_store";

const router = useRouter();

const auth_store = useAuthStore();

onMounted(async () => {
  if (auth_store.autenticado) {
    try {
      const { registrar_push } = await import(
        "@/services/push_notifications_service"
      );

      await registrar_push();
    } catch (error) {
      console.error(
        "No se pudo registrar Push Notifications al recuperar la sesión:",
        error,
      );
    }

    return;
  }

  const inicio_exitoso = await auth_store.iniciar_con_biometria();

  if (inicio_exitoso) {
    await router.replace({
      name: "inicio",
    });
  }
});
</script>
