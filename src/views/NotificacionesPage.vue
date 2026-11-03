<template>
  <comp-page :titulo="'Notificaciones'">
    <div class="syner-notificaciones">
      <section class="syner-notificaciones__header">
        <div>
          <p class="syner-notificaciones__eyebrow">CENTRO DE NOTIFICACIONES</p>

          <h1>Notificaciones</h1>

          <p>
            Consultá tus novedades y mantené al día el estado de tus
            notificaciones.
          </p>
        </div>

        <ion-badge
          v-if="notificaciones_store.cantidad_no_leidas > 0"
          color="primary"
        >
          {{ notificaciones_store.cantidad_no_leidas }} sin leer
        </ion-badge>
      </section>

      <section class="syner-notificaciones__content">
        <div
          v-if="notificaciones_store.cargando"
          class="syner-notificaciones__estado"
        >
          <ion-spinner name="crescent" />

          <p>Cargando notificaciones...</p>
        </div>

        <comp-estado-listado
          v-else-if="notificaciones_store.error"
          tipo="error"
          titulo="No se pudieron cargar las notificaciones"
          :mensaje="notificaciones_store.error"
          reintentar
          @reintentar="cargar"
        />

        <comp-estado-listado
          v-else-if="notificaciones_store.notificaciones.length === 0"
          tipo="vacio"
          titulo="No tenés notificaciones"
          mensaje="Cuando haya novedades, aparecerán acá."
        />

        <ion-list v-else class="syner-notificaciones__list" :inset="true">
          <ion-item
            v-for="notificacion in notificaciones_store.notificaciones"
            :key="notificacion.id"
            button
            :detail="false"
            :class="{
              'syner-notificaciones__item--no-leida': !notificacion.leida,
            }"
            @click="abrir_notificacion(notificacion)"
          >
            <div
              slot="start"
              class="syner-notificaciones__icon"
              :class="{
                'syner-notificaciones__icon--no-leida': !notificacion.leida,
              }"
            >
              <ion-icon :icon="icono_notificacion(notificacion)" />
            </div>

            <ion-label>
              <div class="syner-notificaciones__item-header">
                <h2>{{ notificacion.titulo }}</h2>

                <span>
                  {{ formatear_fecha(notificacion.fechaCreacion) }}
                </span>
              </div>

              <p>{{ notificacion.mensaje }}</p>

              <small v-if="!notificacion.leida"> Nueva </small>
            </ion-label>

            <ion-icon
              v-if="!notificacion.leida"
              slot="end"
              :icon="ellipseOutline"
              class="syner-notificaciones__unread"
            />
          </ion-item>
        </ion-list>
      </section>

      <ion-toast
        :is-open="toast_abierto"
        :message="toast_mensaje"
        :duration="2500"
        @didDismiss="toast_abierto = false"
      />
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import {
  IonBadge,
  IonIcon,
  IonItem,
  IonLabel,
  IonList,
  IonSpinner,
  IonToast,
} from "@ionic/vue";
import {
  ellipseOutline,
  layersOutline,
  notificationsOutline,
  receiptOutline,
} from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import { useNotificacionesStore } from "@/stores/notificaciones_store";
import type { Notificacion } from "@/models/notificacion";

const notificaciones_store = useNotificacionesStore();
const router = useRouter();

const toast_abierto = ref(false);
const toast_mensaje = ref("");

onMounted(() => {
  cargar();
});

async function cargar() {
  try {
    await notificaciones_store.cargar_notificaciones();
  } catch {
    // El store ya administra el mensaje de error.
  }
}

async function abrir_notificacion(notificacion: Notificacion) {
  try {
    if (!notificacion.leida) {
      await notificaciones_store.marcar_leida(notificacion.id);
    }

    if (notificacion.entidadTipo === "lote" && notificacion.entidadId) {
      await router.push({
        name: "lote-detalle",
        params: { id: notificacion.entidadId },
      });
      return;
    }

    if (notificacion.entidadTipo === "pedido" && notificacion.entidadId) {
      await router.push({
        name: "pedidos",
        query: { pedidoId: notificacion.entidadId },
      });
    }
  } catch (error) {
    toast_mensaje.value =
      error instanceof Error
        ? error.message
        : "No se pudo actualizar la notificación.";

    toast_abierto.value = true;
  }
}

function icono_notificacion(notificacion: Notificacion) {
  switch (notificacion.tipo) {
    case "pedido_nuevo":
      return receiptOutline;

    case "pedido_creado":
      return receiptOutline;

    case "lote_disponible":
      return layersOutline;

    case "lote_actualizado":
      return layersOutline;

    default:
      return notificationsOutline;
  }
}

function formatear_fecha(fecha: string) {
  const fecha_notificacion = new Date(fecha);

  if (Number.isNaN(fecha_notificacion.getTime())) {
    return "";
  }

  return new Intl.DateTimeFormat("es-AR", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(fecha_notificacion);
}
</script>

<style scoped>
.syner-notificaciones {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-notificaciones__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  padding: 24px 0 28px;
}

.syner-notificaciones__eyebrow {
  margin: 0 0 8px;
  color: var(--syner-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.syner-notificaciones__header h1 {
  margin: 0;
  color: var(--syner-text);
  font-size: clamp(30px, 7vw, 48px);
  font-weight: 800;
  line-height: 1.05;
}

.syner-notificaciones__header p {
  max-width: 650px;
  margin: 16px 0 0;
  color: var(--syner-text-secondary);
  line-height: 1.5;
}

.syner-notificaciones__content {
  min-height: 250px;
}

.syner-notificaciones__estado {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  min-height: 300px;
  padding: 32px;
  text-align: center;
}

.syner-notificaciones__estado ion-icon {
  color: var(--syner-primary);
  font-size: 48px;
}

.syner-notificaciones__estado h2,
.syner-notificaciones__estado p {
  margin: 0;
}

.syner-notificaciones__estado h2 {
  color: var(--syner-text);
  font-size: 20px;
}

.syner-notificaciones__estado p {
  color: var(--syner-text-secondary);
}

.syner-notificaciones__list {
  margin: 0;
}

.syner-notificaciones__item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-notificaciones__item--no-leida {
  --background: color-mix(
    in srgb,
    var(--syner-primary) 7%,
    var(--syner-surface)
  );
}

.syner-notificaciones__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 42px;
  height: 42px;
  border-radius: 12px;
  background: var(--syner-surface);
  color: var(--syner-text-secondary);
}

.syner-notificaciones__icon--no-leida {
  color: var(--syner-primary);
}

.syner-notificaciones__icon ion-icon {
  font-size: 22px;
}

.syner-notificaciones__item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.syner-notificaciones__item-header h2 {
  margin: 0;
  color: var(--syner-text);
  font-size: 15px;
  font-weight: 700;
}

.syner-notificaciones__item-header span {
  flex-shrink: 0;
  color: var(--syner-text-secondary);
  font-size: 11px;
}

.syner-notificaciones__item ion-label p {
  margin-top: 6px;
  color: var(--syner-text-secondary);
  white-space: normal;
  line-height: 1.4;
}

.syner-notificaciones__item ion-label small {
  display: inline-block;
  margin-top: 6px;
  color: var(--syner-primary);
  font-size: 11px;
  font-weight: 700;
}

.syner-notificaciones__unread {
  color: var(--syner-primary);
  font-size: 10px;
}
</style>
