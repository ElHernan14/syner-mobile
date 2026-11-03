<template>
  <ion-page>
    <ion-content :fullscreen="true">
      <div class="syner-login">
        <div class="syner-login__container">
          <div class="syner-login__header">
            <h1>SYNER</h1>

            <p>Ingresá a tu cuenta para continuar. Los campos con * son obligatorios.</p>
          </div>

          <form class="syner-login__form" novalidate @submit.prevent="iniciar_sesion">
            <div class="syner-login__field">
            <ion-item>
              <ion-input
                v-model="correo"
                type="email"
                label="Correo *"
                label-placement="stacked"
                placeholder="correo@ejemplo.com"
                autocomplete="email"
                required
                :aria-invalid="!!errores.correo"
                :disabled="cargando"
                @ion-input="errores.correo = ''"
              />
            </ion-item>
            <p v-if="errores.correo" class="syner-login__field-error" role="alert">{{ errores.correo }}</p>
            </div>

            <div class="syner-login__field">
            <ion-item>
              <ion-input
                v-model="password"
                type="password"
                label="Contraseña *"
                label-placement="stacked"
                placeholder="Ingresá tu contraseña"
                autocomplete="current-password"
                required
                :aria-invalid="!!errores.password"
                :disabled="cargando"
                @ion-input="errores.password = ''"
              />
            </ion-item>
            <p v-if="errores.password" class="syner-login__field-error" role="alert">{{ errores.password }}</p>
            </div>

            <div v-if="error" class="syner-login__error" role="alert">
              {{ error }}
            </div>

            <ion-button
              class="syner-login__submit"
              type="submit"
              expand="block"
              :disabled="cargando"
            >
              <ion-spinner v-if="cargando" name="crescent" />

              <span v-else>Ingresar</span>
            </ion-button>
          </form>
        </div>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue";

import {
  IonButton,
  IonContent,
  IonInput,
  IonItem,
  IonPage,
  IonSpinner,
} from "@ionic/vue";

import { useRouter } from "vue-router";

import { useAuth } from "@/composables/useAuth";

const router = useRouter();

const { cargando, error: error_auth, iniciar_sesion: iniciar_sesion_auth } = useAuth();

const correo = ref("");
const password = ref("");
const errores = reactive({ correo: "", password: "" });
const error = computed(() => error_auth.value || "");

async function iniciar_sesion() {
  errores.correo = "";
  errores.password = "";
  const correo_limpio = correo.value.trim();

  if (!correo_limpio) errores.correo = "El correo es obligatorio.";
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo_limpio)) {
    errores.correo = "Ingresá un correo válido (ej. nombre@dominio.com).";
  }
  if (!password.value.trim()) errores.password = "La contraseña es obligatoria.";
  if (errores.correo || errores.password) return;

  try {
    await iniciar_sesion_auth({ correo: correo_limpio, password: password.value });
    await router.replace("/app/inicio");
  } catch {
    // El store conserva y muestra el error de autenticación.
  }

}
</script>

<style scoped>
.syner-login {
  min-height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.syner-login__container {
  width: min(100%, 420px);
}

.syner-login__header {
  margin-bottom: 32px;
  text-align: center;
}

.syner-login__header h1 {
  margin: 0 0 8px;
  color: var(--syner-primary);
  font-size: 32px;
  font-weight: 800;
  letter-spacing: 0.08em;
}

.syner-login__header p {
  margin: 0;
  color: var(--syner-text-secondary);
}

.syner-login__form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.syner-login__field { display: grid; gap: 6px; }

.syner-login ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
  --highlight-color-focused: var(--syner-primary);
  border-radius: 10px;
}

.syner-login__field-error {
  margin: -10px 8px 0;
  color: var(--ion-color-danger);
  font-size: 13px;
}

.syner-login__error {
  padding: 12px 14px;
  border: 1px solid #ef4444;
  border-radius: 10px;
  background: rgba(239, 68, 68, 0.08);
  color: #ef4444;
  font-size: 14px;
}

.syner-login__submit {
  margin-top: 8px;
}

.syner-login__submit ion-spinner {
  width: 20px;
  height: 20px;
}
</style>
