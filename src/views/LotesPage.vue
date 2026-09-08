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
        <comp-lote-card
          v-for="lote in lotes"
          :key="lote.id"
          :lote="lote"
          @editar="editar_lote"
          @eliminar="eliminar_lote_confirmar"
        />
      </div>

      <comp-paginacion
        :pagina="page"
        :total_paginas="total_paginas"
        @cambiar="cambiar_pagina"
      />
    </div>

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

import { addOutline as add_outline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompPaginacion from "@/components/shared/comp_paginacion.vue";
import CompLoteCard from "@/components/lotes/comp_lote_card.vue";
import CompLoteFiltros from "@/components/lotes/comp_lote_filtros.vue";
import CompLoteFormulario from "@/components/lotes/comp_lote_formulario.vue";

import { useLotes } from "@/composables/useLotes";
import { useProveedores } from "@/composables/useProveedores";

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

  .syner-lotes__formulario {
    padding: 4px 12px 20px;
  }
}
</style>
