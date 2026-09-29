<template>
  <comp-page titulo="Escanear QR">
    <div class="syner-scanner">
      <div class="syner-scanner__hero">
        <div class="syner-scanner__icon">
          <ion-icon :icon="qr_code_outline" />
        </div>

        <h1>Escanear lote</h1>

        <p>Escaneá el código QR de un lote para consultar su información.</p>
      </div>

      <div class="syner-scanner__card">
        <ion-icon class="syner-scanner__card-icon" :icon="scan_outline" />

        <strong> Código QR de SYNER </strong>

        <span> El código debe pertenecer a un lote de SYNER. </span>

        <ion-button
          expand="block"
          :disabled="escaneando"
          @click="iniciar_escaneo"
        >
          <ion-spinner v-if="escaneando" slot="start" />

          <ion-icon v-else :icon="qr_code_outline" slot="start" />

          {{ escaneando ? "Abriendo cámara..." : "Escanear QR" }}
        </ion-button>
      </div>

      <div v-if="error" class="syner-scanner__error">
        <ion-icon :icon="alert_circle_outline" />

        <span>{{ error }}</span>
      </div>
    </div>

    <ion-toast
      :is-open="mostrar_toast"
      :message="mensaje_toast"
      :duration="3000"
      @did-dismiss="mostrar_toast = false"
    />
  </comp-page>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { IonButton, IonIcon, IonSpinner, IonToast } from "@ionic/vue";

import {
  alertCircleOutline as alert_circle_outline,
  qrCodeOutline as qr_code_outline,
  scanOutline as scan_outline,
} from "ionicons/icons";

import { useRouter } from "vue-router";

import CompPage from "@/components/shared/comp_page.vue";

import { escanear_qr } from "@/services/scanner_service";

import { vibrar_exito, vibrar_error } from "@/services/vibracion_service";

const router = useRouter();

const escaneando = ref(false);

const error = ref<string | null>(null);

const mostrar_toast = ref(false);

const mensaje_toast = ref("");

function mostrar_error(mensaje: string) {
  error.value = mensaje;
  mensaje_toast.value = mensaje;
  mostrar_toast.value = true;
}

function obtener_id_lote(contenido: string): number | null {
  const coincidencia = contenido.match(
    /^https:\/\/syner\.app\/lotes\/(\d+)\/?$/,
  );

  if (!coincidencia) {
    return null;
  }

  const id = Number(coincidencia[1]);

  if (!Number.isInteger(id) || id <= 0) {
    return null;
  }

  return id;
}

async function iniciar_escaneo() {
  if (escaneando.value) {
    return;
  }

  escaneando.value = true;
  error.value = null;

  try {
    const resultado = await escanear_qr();

    const lote_id = obtener_id_lote(resultado.contenido);

    if (lote_id === null) {
      await vibrar_error();

      mostrar_error("El código QR no corresponde a un lote de SYNER.");

      return;
    }

    await vibrar_exito();

    await router.push({
      name: "lote-detalle",
      params: {
        id: String(lote_id),
      },
    });
  } catch (err) {
    const mensaje =
      err instanceof Error ? err.message : "No se pudo realizar el escaneo.";

    await vibrar_error();

    mostrar_error(mensaje);
  } finally {
    escaneando.value = false;
  }
}
</script>

<style scoped>
.syner-scanner {
  width: min(100% - 32px, 720px);
  margin: 0 auto;
  padding: 32px 0 48px;
}

.syner-scanner__hero {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.syner-scanner__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 72px;
  height: 72px;
  margin-bottom: 18px;
  border-radius: 20px;
  background: var(--syner-primary);
  color: #ffffff;
}

.syner-scanner__icon ion-icon {
  font-size: 38px;
}

.syner-scanner__hero h1 {
  margin: 0;
  color: var(--syner-text);
  font-size: 25px;
  font-weight: 700;
}

.syner-scanner__hero p {
  max-width: 460px;
  margin: 10px 0 0;
  color: var(--syner-text-secondary);
  font-size: 14px;
  line-height: 1.6;
}

.syner-scanner__card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  margin-top: 32px;
  padding: 28px 24px;
  border: 1px solid var(--syner-border);
  border-radius: 16px;
  background: var(--syner-surface);
  text-align: center;
}

.syner-scanner__card-icon {
  margin-bottom: 4px;
  color: var(--syner-primary);
  font-size: 42px;
}

.syner-scanner__card strong {
  color: var(--syner-text);
  font-size: 16px;
}

.syner-scanner__card span {
  margin-bottom: 12px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-scanner__card ion-button {
  width: 100%;
  max-width: 360px;
}

.syner-scanner__error {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 20px;
  padding: 14px 16px;
  border: 1px solid var(--syner-border);
  border-radius: 12px;
  color: var(--syner-text-secondary);
  text-align: center;
}

.syner-scanner__error ion-icon {
  flex-shrink: 0;
  color: var(--syner-primary);
  font-size: 20px;
}
</style>
