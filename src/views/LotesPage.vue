<template>
  <comp-page
    titulo="Lotes"
    :mostrar_refresher="true"
    @actualizar="cargar_lotes"
  >
    <div class="syner-lotes">
      <div class="syner-lotes__header">
        <div>
          <h1>Lotes</h1>
          <p>Gestioná los lotes disponibles en SYNER.</p>
        </div>

        <ion-button @click="nuevo_lote">
          <ion-icon :icon="add_outline" slot="start" />
          Nuevo lote
        </ion-button>
      </div>

      <comp-lote-filtros :termino="termino" @buscar="buscar_lotes" />

      <div v-if="cargando" class="syner-lotes__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <div v-else-if="error" class="syner-lotes__error">
        <p>{{ error }}</p>

        <ion-button fill="outline" @click="cargar_lotes">
          Reintentar
        </ion-button>
      </div>

      <div v-else-if="!hay_lotes" class="syner-lotes__empty">
        <p>
          {{
            termino ? "No se encontraron lotes." : "No hay lotes disponibles."
          }}
        </p>

        <ion-button v-if="!termino" @click="nuevo_lote">
          Crear primer lote
        </ion-button>
      </div>

      <div v-else class="syner-lotes__list">
        <div v-for="lote in lotes" :key="lote.id" class="syner-lotes__item">
          <comp-lote-card
            :lote="lote"
            @editar="editar_lote"
            @eliminar="eliminar_lote_confirmar"
          />

          <div class="syner-lotes__acciones">
            <ion-button
              fill="outline"
              size="small"
              :disabled="lote_pdf_cargando === lote.id"
              @click="ver_pdf_lote(lote.id)"
            >
              <ion-spinner v-if="lote_pdf_cargando === lote.id" slot="start" />

              <ion-icon v-else :icon="document_text_outline" slot="start" />

              {{
                lote_pdf_cargando === lote.id ? "Generando..." : "Ver ficha PDF"
              }}
            </ion-button>

            <ion-button
              fill="outline"
              size="small"
              :disabled="lote_qr_cargando === lote.id"
              @click="ver_qr_lote(lote.id)"
            >
              <ion-spinner v-if="lote_qr_cargando === lote.id" slot="start" />

              <ion-icon v-else :icon="qr_code_outline" slot="start" />

              {{ lote_qr_cargando === lote.id ? "Generando..." : "Ver QR" }}
            </ion-button>

            <ion-button
              fill="outline"
              size="small"
              :disabled="lote_pdf_compartiendo === lote.id"
              @click="compartir_pdf_lote(lote.id)"
            >
              <ion-spinner
                v-if="lote_pdf_compartiendo === lote.id"
                slot="start"
              />

              <ion-icon v-else :icon="share_outline" slot="start" />

              {{
                lote_pdf_compartiendo === lote.id
                  ? "Preparando..."
                  : "Compartir PDF"
              }}
            </ion-button>
          </div>
        </div>
      </div>

      <comp-paginacion
        :pagina="page"
        :total_paginas="total_paginas"
        @cambiar="cambiar_pagina"
      />
    </div>

    <!-- Modal formulario -->
    <ion-modal :is-open="mostrar_formulario" @did-dismiss="cerrar_formulario">
      <ion-header>
        <ion-toolbar>
          <ion-title>
            {{ lote_seleccionado ? "Editar lote" : "Nuevo lote" }}
          </ion-title>

          <ion-buttons slot="end">
            <ion-button :disabled="procesando" @click="cerrar_formulario">
              Cerrar
            </ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>

      <ion-content>
        <div class="syner-lotes__formulario">
          <div
            v-if="cargando_proveedores"
            class="syner-lotes__proveedores-loading"
          >
            <ion-spinner />
            <span>Cargando proveedores...</span>
          </div>

          <div
            v-else-if="error_proveedores"
            class="syner-lotes__proveedores-error"
          >
            <p>{{ error_proveedores }}</p>

            <ion-button fill="outline" size="small" @click="cargar_proveedores">
              Reintentar
            </ion-button>
          </div>

          <comp-lote-formulario
            v-else
            :lote="lote_seleccionado"
            :proveedores="proveedores"
            :procesando="procesando"
            :error="error_formulario"
            @guardar="guardar_lote"
            @cancelar="cerrar_formulario"
          />
        </div>
      </ion-content>
    </ion-modal>

    <ion-modal :is-open="mostrar_qr" @did-dismiss="cerrar_qr">
      <ion-header>
        <ion-toolbar>
          <ion-title> Código QR </ion-title>

          <ion-buttons slot="end">
            <ion-button @click="cerrar_qr"> Cerrar </ion-button>
          </ion-buttons>
        </ion-toolbar>
      </ion-header>

      <ion-content class="syner-lotes__qr-content">
        <div v-if="qr_url" class="syner-lotes__qr">
          <img :src="qr_url" :alt="`Código QR del lote ${qr_lote_id}`" />

          <p>Escaneá este código para acceder al lote.</p>

          <span> Lote #{{ qr_lote_id }} </span>
        </div>

        <div v-else class="syner-lotes__qr-empty">
          <ion-spinner />
          <p>Preparando código QR...</p>
        </div>
      </ion-content>
    </ion-modal>

    <ion-alert
      :is-open="mostrar_confirmacion_baja"
      header="Dar de baja lote"
      :message="mensaje_baja"
      :buttons="botones_baja"
      @did-dismiss="cerrar_confirmacion_baja"
    />

    <ion-toast
      :is-open="mostrar_toast"
      :message="mensaje_toast"
      :color="color_toast"
      :duration="3000"
      position="bottom"
      @did-dismiss="cerrar_toast"
    />
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

import {
  IonAlert,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonIcon,
  IonModal,
  IonSpinner,
  IonTitle,
  IonToast,
  IonToolbar,
} from "@ionic/vue";

import {
  addOutline as add_outline,
  documentTextOutline as document_text_outline,
  qrCodeOutline as qr_code_outline,
  shareOutline as share_outline,
} from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompPaginacion from "@/components/shared/comp_paginacion.vue";
import CompLoteCard from "@/components/lotes/comp_lote_card.vue";
import CompLoteFiltros from "@/components/lotes/comp_lote_filtros.vue";
import CompLoteFormulario from "@/components/lotes/comp_lote_formulario.vue";

import { useLotes } from "@/composables/useLotes";
import { useProveedores } from "@/composables/useProveedores";

import { obtener_pdf_lote, obtener_qr_lote } from "@/services/lotes_service";
import { abrir_pdf, compartir_pdf } from "@/services/archivo_service";

import type { Lote } from "@/models/LoteModel";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";

const {
  lotes,
  cargando,
  procesando,
  error,
  hay_lotes,
  page,
  total_paginas,
  cargar_lotes,
  crear_lote,
  actualizar_lote,
  eliminar_lote,
} = useLotes();

const {
  proveedores,
  cargando: cargando_proveedores,
  error: error_proveedores,
  cargar_proveedores,
} = useProveedores();

const termino = ref("");

const mostrar_formulario = ref(false);
const lote_seleccionado = ref<Lote | null>(null);
const error_formulario = ref<string | null>(null);

const mostrar_confirmacion_baja = ref(false);
const lote_para_eliminar = ref<Lote | null>(null);
const mensaje_baja = ref("");

const mostrar_toast = ref(false);
const mensaje_toast = ref("");
const color_toast = ref<"success" | "danger">("success");

const lote_pdf_cargando = ref<number | null>(null);

const lote_qr_cargando = ref<number | null>(null);
const mostrar_qr = ref(false);
const qr_url = ref<string | null>(null);
const qr_lote_id = ref<number | null>(null);

const lote_pdf_compartiendo = ref<number | null>(null);

let temporizador_busqueda: ReturnType<typeof setTimeout> | undefined;

function mostrar_toast_exito(mensaje: string) {
  mensaje_toast.value = mensaje;
  color_toast.value = "success";
  mostrar_toast.value = true;
}

function mostrar_toast_error(mensaje: string) {
  mensaje_toast.value = mensaje;
  color_toast.value = "danger";
  mostrar_toast.value = true;
}

function cerrar_toast() {
  mostrar_toast.value = false;
}

function buscar_lotes(valor: string) {
  termino.value = valor;

  if (temporizador_busqueda) {
    clearTimeout(temporizador_busqueda);
  }

  temporizador_busqueda = setTimeout(() => {
    cargar_lotes(1, termino.value);
  }, 400);
}

function cambiar_pagina(nueva_pagina: number) {
  cargar_lotes(nueva_pagina, termino.value);
}

function nuevo_lote() {
  lote_seleccionado.value = null;
  error_formulario.value = null;
  mostrar_formulario.value = true;

  if (proveedores.value.length === 0) {
    cargar_proveedores();
  }
}

function editar_lote(lote: Lote) {
  lote_seleccionado.value = lote;
  error_formulario.value = null;
  mostrar_formulario.value = true;

  if (proveedores.value.length === 0) {
    cargar_proveedores();
  }
}

async function guardar_lote(request: CrearLoteRequest | ActualizarLoteRequest) {
  error_formulario.value = null;

  const es_edicion = lote_seleccionado.value !== null;

  try {
    if (es_edicion) {
      await actualizar_lote(
        lote_seleccionado.value!.id,
        request as ActualizarLoteRequest,
      );

      mostrar_toast_exito("El lote se actualizó correctamente.");
    } else {
      await crear_lote(request as CrearLoteRequest);

      mostrar_toast_exito("El lote se creó correctamente.");
    }

    cerrar_formulario();

    await cargar_lotes(page.value, termino.value);
  } catch (err) {
    const mensaje =
      err instanceof Error ? err.message : "No se pudo guardar el lote.";

    error_formulario.value = mensaje;
    mostrar_toast_error(mensaje);
  }
}

function cerrar_formulario() {
  if (procesando.value) {
    return;
  }

  mostrar_formulario.value = false;
  lote_seleccionado.value = null;
  error_formulario.value = null;
}

async function ver_pdf_lote(id: number) {
  if (lote_pdf_cargando.value !== null) {
    return;
  }

  lote_pdf_cargando.value = id;

  try {
    const blob = await obtener_pdf_lote(id);

    await abrir_pdf(blob, `syner-lote-${id}.pdf`);
  } catch (err) {
    const mensaje =
      err instanceof Error ? err.message : "No se pudo abrir la ficha PDF.";

    mostrar_toast_error(mensaje);
  } finally {
    lote_pdf_cargando.value = null;
  }
}

function eliminar_lote_confirmar(lote: Lote) {
  lote_para_eliminar.value = lote;

  mensaje_baja.value =
    `¿Estás seguro de que querés dar de baja el lote "${lote.nombre}"? ` +
    "El lote no se eliminará físicamente y quedará como cancelado.";

  mostrar_confirmacion_baja.value = true;
}

function cerrar_confirmacion_baja() {
  mostrar_confirmacion_baja.value = false;
  lote_para_eliminar.value = null;
}

async function confirmar_baja() {
  if (!lote_para_eliminar.value) {
    return;
  }

  const id = lote_para_eliminar.value.id;

  try {
    await eliminar_lote(id);

    cerrar_confirmacion_baja();

    mostrar_toast_exito("El lote se dio de baja correctamente.");

    await cargar_lotes(page.value, termino.value);
  } catch (err) {
    const mensaje =
      err instanceof Error ? err.message : "No se pudo dar de baja el lote.";

    cerrar_confirmacion_baja();
    mostrar_toast_error(mensaje);
  }
}

const botones_baja = [
  {
    text: "Cancelar",
    role: "cancel",
    handler: cerrar_confirmacion_baja,
  },
  {
    text: "Dar de baja",
    role: "destructive",
    handler: confirmar_baja,
  },
];

async function ver_qr_lote(id: number) {
  if (lote_qr_cargando.value !== null) {
    return;
  }

  lote_qr_cargando.value = id;

  try {
    const blob = await obtener_qr_lote(id);

    if (qr_url.value) {
      URL.revokeObjectURL(qr_url.value);
    }

    qr_url.value = URL.createObjectURL(blob);
    qr_lote_id.value = id;
    mostrar_qr.value = true;
  } catch (err) {
    const mensaje =
      err instanceof Error ? err.message : "No se pudo generar el código QR.";

    mostrar_toast_error(mensaje);
  } finally {
    lote_qr_cargando.value = null;
  }
}

function cerrar_qr() {
  mostrar_qr.value = false;
  qr_lote_id.value = null;

  if (qr_url.value) {
    URL.revokeObjectURL(qr_url.value);
    qr_url.value = null;
  }
}

async function compartir_pdf_lote(id: number) {
  if (lote_pdf_compartiendo.value !== null) {
    return;
  }

  lote_pdf_compartiendo.value = id;

  try {
    const blob = await obtener_pdf_lote(id);

    await compartir_pdf(blob, `syner-lote-${id}.pdf`);
  } catch (error) {
    const mensaje =
      error instanceof Error ? error.message : "No se pudo compartir el PDF.";

    mostrar_toast_error(mensaje);
  } finally {
    lote_pdf_compartiendo.value = null;
  }
}

onMounted(() => {
  cargar_lotes();
});
</script>

<style scoped>
.syner-lotes {
  width: 100%;
  max-width: 1000px;
  margin: 0 auto;
  padding: 16px;
}

.syner-lotes__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.syner-lotes__header h1 {
  margin: 0;
  color: var(--syner-text);
  font-size: 24px;
  font-weight: 700;
}

.syner-lotes__header p {
  margin: 4px 0 0;
  color: var(--syner-text-secondary);
  font-size: 14px;
}

.syner-lotes__loading {
  margin-top: 16px;
}

.syner-lotes__error,
.syner-lotes__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  min-height: 220px;
  padding: 32px;
  text-align: center;
  color: var(--syner-text-secondary);
}

.syner-lotes__error p,
.syner-lotes__empty p {
  margin: 0;
}

.syner-lotes__list {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 16px;
}

.syner-lotes__item {
  min-width: 0;
}

.syner-lotes__acciones {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: -8px;
  padding: 0 4px;
}

.syner-lotes__acciones ion-button {
  margin: 0;
}

.syner-lotes__formulario {
  width: 100%;
  padding: 8px 16px 24px;
}

.syner-lotes__proveedores-loading,
.syner-lotes__proveedores-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  min-height: 180px;
  padding: 24px;
  text-align: center;
  color: var(--syner-text-secondary);
}

.syner-lotes__proveedores-loading ion-spinner {
  width: 32px;
  height: 32px;
}

.syner-lotes__proveedores-loading span {
  font-size: 14px;
}

.syner-lotes__proveedores-error p {
  margin: 0;
  color: var(--ion-color-danger);
  font-size: 14px;
}

@media (max-width: 768px) {
  .syner-lotes {
    padding: 12px;
  }

  .syner-lotes__header {
    align-items: flex-start;
    flex-direction: column;
  }

  .syner-lotes__header ion-button {
    width: 100%;
  }

  .syner-lotes__list {
    grid-template-columns: 1fr;
  }

  .syner-lotes__acciones {
    flex-direction: column;
    justify-content: stretch;
    padding: 0;
  }

  .syner-lotes__acciones ion-button {
    width: 100%;
    margin: 0;
  }

  .syner-lotes__formulario {
    padding: 4px 12px 20px;
  }
}

.syner-lotes__qr-content {
  --background: var(--syner-background);
}

.syner-lotes__qr {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  min-height: 100%;
  padding: 32px 20px;
  text-align: center;
}

.syner-lotes__qr img {
  width: min(280px, 80vw);
  height: auto;
  padding: 16px;
  border-radius: 12px;
  background: white;
}

.syner-lotes__qr p {
  margin: 8px 0 0;
  color: var(--syner-text-secondary);
  font-size: 14px;
}

.syner-lotes__qr span {
  color: var(--syner-text);
  font-size: 13px;
  font-weight: 600;
}

.syner-lotes__qr-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  min-height: 300px;
  color: var(--syner-text-secondary);
}
</style>
