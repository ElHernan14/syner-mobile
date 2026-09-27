<template>
  <comp-page :titulo="'Mi Cuenta'">
    <div class="syner-mi-cuenta">
      <ion-list :inset="true" class="syner-mi-cuenta__perfil">
        <ion-list-header>
          <ion-label>Perfil</ion-label>
        </ion-list-header>

        <div class="syner-mi-cuenta__foto">
          <div v-if="foto_perfil" class="syner-mi-cuenta__foto-preview">
            <img :src="foto_perfil" alt="Foto de perfil" />
          </div>

          <div v-else class="syner-mi-cuenta__foto-vacia">
            <ion-icon :icon="cameraOutline" />
            <span>Sin foto</span>
          </div>
        </div>

        <div class="syner-mi-cuenta__foto-acciones">
          <ion-button
            fill="outline"
            :disabled="procesando_foto"
            @click="sacar_foto_perfil"
          >
            <ion-icon slot="start" :icon="cameraOutline" />
            CÁMARA
          </ion-button>

          <ion-button
            fill="outline"
            :disabled="procesando_foto"
            @click="elegir_foto_perfil"
          >
            <ion-icon slot="start" :icon="imagesOutline" />
            GALERÍA
          </ion-button>
        </div>
      </ion-list>

      <div
        v-if="mensaje_foto"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': mensaje_foto.estado === 'ok',
          'syner-mi-cuenta__resultado--error': mensaje_foto.estado === 'error',
        }"
      >
        {{ mensaje_foto.mensaje }}
      </div>

      <ion-list :inset="true">
        <ion-item>
          <ion-icon slot="start" :icon="pulseOutline" />

          <ion-label>
            <h2>Diagnóstico</h2>

            <p>Esta app consulta los datos desde:</p>

            <p class="syner-mi-cuenta__url">
              {{ api_url }}
            </p>
          </ion-label>
        </ion-item>
      </ion-list>

      <div class="syner-mi-cuenta__accion">
        <ion-button
          expand="block"
          :disabled="probando"
          @click="probar_conexion"
        >
          {{ probando ? "Probando..." : "PROBAR CONEXIÓN" }}
        </ion-button>
      </div>

      <ion-list :inset="true" class="syner-mi-cuenta__seguridad">
        <ion-list-header>
          <ion-label>Seguridad</ion-label>
        </ion-list-header>

        <ion-item>
          <ion-icon slot="start" :icon="fingerPrintOutline" />

          <ion-label>
            <h2>Acceso con huella</h2>

            <p v-if="cargando_biometria">Verificando disponibilidad...</p>

            <p v-else-if="!biometria_disponible_local">
              La biometría no está disponible en este dispositivo.
            </p>

            <p v-else-if="auth_store.biometria_activa">
              La huella está activada para iniciar sesión.
            </p>

            <p v-else>
              Usá la huella del dispositivo para iniciar sesión de forma segura.
            </p>
          </ion-label>

          <ion-toggle
            slot="end"
            :checked="auth_store.biometria_activa"
            :disabled="
              cargando_biometria ||
              !biometria_disponible_local ||
              procesando_biometria
            "
            @ion-change="cambiar_biometria"
          />
        </ion-item>
      </ion-list>

      <div
        v-if="mensaje_biometria"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': mensaje_biometria.estado === 'ok',

          'syner-mi-cuenta__resultado--error':
            mensaje_biometria.estado === 'error',
        }"
      >
        {{ mensaje_biometria.mensaje }}
      </div>

      <ion-list :inset="true" class="syner-mi-cuenta__preferencias">
        <ion-list-header>
          <ion-label>Preferencias</ion-label>
        </ion-list-header>

        <ion-item>
          <ion-icon slot="start" :icon="phonePortraitOutline" />

          <ion-label>
            <h2>Vibración</h2>

            <p v-if="preferencias_store.vibracion_activa">
              SYNER puede vibrar al confirmar acciones importantes.
            </p>

            <p v-else>La vibración está desactivada.</p>
          </ion-label>

          <ion-toggle
            slot="end"
            :checked="preferencias_store.vibracion_activa"
            @ion-change="cambiar_vibracion"
          />
        </ion-item>
      </ion-list>

      <div class="syner-mi-cuenta__accion">
        <ion-button
          expand="block"
          fill="outline"
          :disabled="probando_vibracion || !preferencias_store.vibracion_activa"
          @click="probar_vibracion"
        >
          {{ probando_vibracion ? "Vibrando..." : "PROBAR VIBRACIÓN" }}
        </ion-button>
      </div>

      <div
        v-if="mensaje_vibracion"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': mensaje_vibracion.estado === 'ok',

          'syner-mi-cuenta__resultado--error':
            mensaje_vibracion.estado === 'error',
        }"
      >
        {{ mensaje_vibracion.mensaje }}
      </div>

      <div
        v-if="resultado"
        class="syner-mi-cuenta__resultado"
        :class="{
          'syner-mi-cuenta__resultado--ok': resultado.estado === 'ok',

          'syner-mi-cuenta__resultado--error': resultado.estado === 'error',
        }"
      >
        {{ resultado.mensaje }}
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

import {
  IonButton,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonListHeader,
  IonToggle,
} from "@ionic/vue";

import {
  cameraOutline,
  fingerPrintOutline,
  imagesOutline,
  phonePortraitOutline,
  pulseOutline,
} from "ionicons/icons";

import { elegir_foto_galeria, sacar_foto } from "@/services/camara_service";

import CompPage from "@/components/shared/comp_page.vue";

import { obtener } from "@/services/ajax_service";

import { biometria_disponible } from "@/services/biometria_service";

import { vibrar_exito } from "@/services/vibracion_service";

import { obtener_api_url } from "@/config/debug";

import { useAuthStore } from "@/stores/auth_store";

import { usePreferenciasStore } from "@/stores/preferencias_store";

import { actualizar_avatar } from "@/services/usuarios_service";

interface HealthResponse {
  estado: string;
  mensaje: string;
}

const auth_store = useAuthStore();

const preferencias_store = usePreferenciasStore();

const api_url = obtener_api_url();

const probando = ref(false);

const cargando_biometria = ref(true);

const procesando_biometria = ref(false);

const probando_vibracion = ref(false);

const biometria_disponible_local = ref(false);

const mensaje_biometria = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

const mensaje_vibracion = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

const resultado = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

async function cargar_disponibilidad_biometria() {
  cargando_biometria.value = true;

  try {
    biometria_disponible_local.value = await biometria_disponible();
  } catch {
    biometria_disponible_local.value = false;
  } finally {
    cargando_biometria.value = false;
  }
}

async function cambiar_biometria(event: CustomEvent) {
  const activar = event.detail.checked;

  mensaje_biometria.value = null;

  procesando_biometria.value = true;

  try {
    if (activar) {
      const activada = await auth_store.activar_biometria();

      if (!activada) {
        mensaje_biometria.value = {
          estado: "error",
          mensaje: "No se pudo activar el acceso con huella.",
        };

        return;
      }

      mensaje_biometria.value = {
        estado: "ok",
        mensaje: "Acceso con huella activado correctamente.",
      };

      return;
    }

    await auth_store.desactivar_biometria();

    mensaje_biometria.value = {
      estado: "ok",
      mensaje: "Acceso con huella desactivado.",
    };
  } finally {
    procesando_biometria.value = false;
  }
}

function cambiar_vibracion(event: CustomEvent) {
  const activa = event.detail.checked;

  preferencias_store.establecer_vibracion_activa(activa);

  mensaje_vibracion.value = {
    estado: "ok",
    mensaje: activa ? "Vibración activada." : "Vibración desactivada.",
  };
}

async function probar_vibracion() {
  probando_vibracion.value = true;

  mensaje_vibracion.value = null;

  try {
    await vibrar_exito();

    mensaje_vibracion.value = {
      estado: "ok",
      mensaje: "Vibración ejecutada correctamente.",
    };
  } catch (err) {
    console.error("Error al ejecutar vibración:", err);

    mensaje_vibracion.value = {
      estado: "error",
      mensaje: "No se pudo ejecutar la vibración.",
    };
  } finally {
    probando_vibracion.value = false;
  }
}

async function probar_conexion() {
  probando.value = true;

  resultado.value = null;

  try {
    const respuesta = await obtener<HealthResponse>("health");

    resultado.value = {
      estado: "ok",
      mensaje: respuesta.mensaje,
    };
  } catch (err) {
    console.error("Error al probar conexión:", err);

    resultado.value = {
      estado: "error",
      mensaje: "No se pudo establecer conexión con la API.",
    };
  } finally {
    probando.value = false;
  }
}

const foto_perfil = ref<string | null>(auth_store.usuario?.avatar ?? null);
const procesando_foto = ref(false);

const mensaje_foto = ref<{
  estado: "ok" | "error";
  mensaje: string;
} | null>(null);

async function guardar_foto_perfil(web_path: string) {
  try {
    const respuesta = await fetch(web_path);
    const blob = await respuesta.blob();

    const extension =
      blob.type === "image/png"
        ? "png"
        : blob.type === "image/webp"
        ? "webp"
        : "jpg";

    const archivo = new File([blob], `foto-perfil.${extension}`, {
      type: blob.type || "image/jpeg",
    });

    const resultado = await actualizar_avatar(archivo, archivo.name);

    if (!resultado.exito || !resultado.datos) {
      throw new Error(
        resultado.errores?.[0] ??
          resultado.mensaje ??
          "No se pudo actualizar la foto.",
      );
    }

    if (auth_store.usuario) {
      auth_store.usuario.avatar = resultado.datos.avatar;
    }

    mensaje_foto.value = {
      estado: "ok",
      mensaje: "Foto de perfil guardada correctamente.",
    };
  } catch (err) {
    console.error("Error al guardar foto de perfil:", err);

    mensaje_foto.value = {
      estado: "error",
      mensaje: "No se pudo guardar la foto de perfil.",
    };
  }
}

async function sacar_foto_perfil() {
  procesando_foto.value = true;
  mensaje_foto.value = null;

  try {
    const foto = await sacar_foto();

    foto_perfil.value = foto.web_path ?? null;

    await guardar_foto_perfil(foto.web_path ?? "");
  } catch (err) {
    console.error("Error al sacar foto:", err);

    mensaje_foto.value = {
      estado: "error",
      mensaje: "No se pudo tomar o guardar la foto.",
    };
  } finally {
    procesando_foto.value = false;
  }
}

async function elegir_foto_perfil() {
  procesando_foto.value = true;
  mensaje_foto.value = null;

  try {
    const foto = await elegir_foto_galeria();

    foto_perfil.value = foto.web_path ?? null;

    await guardar_foto_perfil(foto.web_path ?? "");
  } catch (err) {
    console.error("Error al seleccionar foto:", err);

    mensaje_foto.value = {
      estado: "error",
      mensaje: "No se pudo seleccionar o guardar la foto.",
    };
  } finally {
    procesando_foto.value = false;
  }
}

onMounted(async () => {
  await cargar_disponibilidad_biometria();
});
</script>

<style scoped>
.syner-mi-cuenta {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-mi-cuenta ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-mi-cuenta ion-icon {
  color: var(--syner-primary);
}

.syner-mi-cuenta__url {
  margin-top: 8px;
  word-break: break-all;
  color: var(--syner-text);
  font-family: monospace;
  font-size: 13px;
}

.syner-mi-cuenta__accion {
  margin-top: 24px;
}

.syner-mi-cuenta__seguridad {
  margin-top: 24px;
}

.syner-mi-cuenta__preferencias {
  margin-top: 24px;
}

.syner-mi-cuenta__resultado {
  margin-top: 16px;
  padding: 14px;
  border-radius: 8px;
  text-align: center;
  font-size: 14px;
}

.syner-mi-cuenta__resultado--ok {
  color: var(--syner-success);
}

.syner-mi-cuenta__resultado--error {
  color: #ef4444;
}

.syner-mi-cuenta__perfil {
  margin-bottom: 24px;
}

.syner-mi-cuenta__foto {
  display: flex;
  justify-content: center;
  padding: 24px 16px 16px;
}

.syner-mi-cuenta__foto-preview,
.syner-mi-cuenta__foto-vacia {
  width: 140px;
  height: 140px;
  border-radius: 50%;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--syner-surface);
  border: 2px solid var(--syner-border);
}

.syner-mi-cuenta__foto-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.syner-mi-cuenta__foto-vacia {
  flex-direction: column;
  gap: 8px;
  color: var(--syner-text);
}

.syner-mi-cuenta__foto-vacia ion-icon {
  font-size: 42px;
}

.syner-mi-cuenta__foto-acciones {
  display: flex;
  justify-content: center;
  gap: 12px;
  padding: 0 16px 20px;
}
</style>
