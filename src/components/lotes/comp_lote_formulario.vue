```vue
<template>
  <ion-card class="syner-lote-formulario">
    <ion-card-header>
      <ion-card-title>
        {{ modo_edicion ? "Editar lote" : "Nuevo lote" }}
      </ion-card-title>

      <ion-card-subtitle>
        {{
          modo_edicion
            ? "Actualizá los datos del lote."
            : "Completá los datos para crear un nuevo lote."
        }}
      </ion-card-subtitle>
      <p class="syner-lote-formulario__hint">Todos los campos son obligatorios. Si falta un dato o no cumple las reglas, se indica debajo del campo.</p>
    </ion-card-header>

    <ion-card-content>
      <form novalidate @submit.prevent="guardar">
        <div class="syner-lote-formulario__grid">
          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.nombre"
              label="Nombre"
              label-placement="stacked"
              fill="outline"
              placeholder="Ej. Notebook Lenovo"
              required
              @ion-input="limpiar_error_campo('nombre')"
            />

            <span
              v-if="errores.nombre"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.nombre }}
            </span>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.categoria"
              label="Categoría"
              label-placement="stacked"
              fill="outline"
              placeholder="Ej. tecnologia"
              required
              @ion-input="limpiar_error_campo('categoria')"
            />

            <span
              v-if="errores.categoria"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.categoria }}
            </span>
          </div>

          <div class="syner-lote-formulario__field syner-lote-formulario__full">
            <ion-textarea
              v-model="formulario.descripcion"
              label="Descripción"
              label-placement="stacked"
              fill="outline"
              placeholder="Describí el lote..."
              :auto-grow="true"
              required
              @ion-input="limpiar_error_campo('descripcion')"
            />

            <div class="syner-lote-formulario__voz">
              <ion-button
                v-if="voz_disponible"
                type="button"
                fill="outline"
                size="small"
                :disabled="dictando || procesando"
                @click="dictar_descripcion"
              >
                <ion-icon slot="start" :icon="dictando ? micOffOutline : micOutline" />
                {{ dictando ? "Escuchando…" : "Dictar descripción" }}
              </ion-button>
              <span v-else>Podés completar la descripción con el teclado.</span>
            </div>
            <span v-if="error_voz" class="syner-lote-formulario__field-error" role="status">{{ error_voz }}</span>

            <span
              v-if="errores.descripcion"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.descripcion }}
            </span>
          </div>

          <div class="syner-lote-formulario__field syner-lote-formulario__full">
            <label class="syner-lote-formulario__image-label" for="lote-imagen-avatar">
              Imagen del lote <span>Opcional · JPG, PNG o WEBP · máximo 5 MB</span>
            </label>
            <input
              id="lote-imagen-avatar"
              class="syner-lote-formulario__image-input"
              type="file"
              accept="image/jpeg,image/png,image/webp"
              :disabled="procesando"
              @change="seleccionar_imagen"
            />
            <p v-if="error_imagen" class="syner-lote-formulario__field-error" role="alert">
              {{ error_imagen }}
            </p>
            <div v-if="imagen_previa" class="syner-lote-formulario__image-preview">
              <img :src="imagen_previa" alt="Vista previa de la imagen del lote" />
              <ion-button
                v-if="imagen_seleccionada"
                type="button"
                fill="clear"
                size="small"
                :disabled="procesando"
                @click="limpiar_imagen_seleccionada"
              >
                Descartar nueva imagen
              </ion-button>
              <span v-else>Imagen actual del lote</span>
            </div>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.precioMercado"
              label="Precio de mercado"
              label-placement="stacked"
              fill="outline"
              type="number"
              min="0"
              step="0.01"
              required
              @ion-input="limpiar_error_campo('precioMercado')"
            />

            <span
              v-if="errores.precioMercado"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.precioMercado }}
            </span>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.precioCupo"
              label="Precio de cupo"
              label-placement="stacked"
              fill="outline"
              type="number"
              min="0"
              step="0.01"
              required
              @ion-input="limpiar_error_campo('precioCupo')"
            />

            <span
              v-if="errores.precioCupo"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.precioCupo }}
            </span>
          </div>

          <div class="syner-lote-formulario__ahorro" aria-live="polite">
            <span>Ahorro calculado automáticamente</span>
            <strong>{{ porcentaje_ahorro_calculado }}%</strong>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.cantidadCupos"
              label="Cantidad de cupos"
              label-placement="stacked"
              fill="outline"
              type="number"
              min="1"
              step="1"
              required
              @ion-input="limpiar_error_campo('cantidadCupos')"
            />

            <span
              v-if="errores.cantidadCupos"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.cantidadCupos }}
            </span>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.fechaInicio"
              label="Fecha de inicio"
              label-placement="stacked"
              fill="outline"
              type="date"
              required
              @ion-input="limpiar_error_campo('fechaInicio')"
            />

            <span
              v-if="errores.fechaInicio"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.fechaInicio }}
            </span>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-input
              v-model="formulario.fechaFin"
              label="Fecha de finalización"
              label-placement="stacked"
              fill="outline"
              type="date"
              required
              @ion-input="limpiar_error_campo('fechaFin')"
            />

            <span
              v-if="errores.fechaFin"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.fechaFin }}
            </span>
          </div>

          <div class="syner-lote-formulario__field">
            <ion-select
              v-model="formulario.proveedorId"
              label="Proveedor"
              label-placement="stacked"
              fill="outline"
              interface="popover"
              placeholder="Seleccioná un proveedor"
              required
              @ion-change="limpiar_error_campo('proveedorId')"
            >
              <ion-select-option
                v-for="proveedor in proveedores"
                :key="proveedor.id"
                :value="proveedor.id"
              >
                {{ proveedor.nombre }}
              </ion-select-option>
            </ion-select>

            <span
              v-if="errores.proveedorId"
              class="syner-lote-formulario__field-error"
            >
              {{ errores.proveedorId }}
            </span>
          </div>
        </div>

        <p v-if="error" class="syner-lote-formulario__error" role="alert">
          {{ error }}
        </p>

        <div class="syner-lote-formulario__actions">
          <ion-button
            type="button"
            fill="outline"
            :disabled="procesando"
            @click="cancelar"
          >
            Cancelar
          </ion-button>

          <ion-button type="submit" :disabled="procesando">
            {{ procesando ? "Guardando..." : "Guardar" }}
          </ion-button>
        </div>
      </form>
    </ion-card-content>
  </ion-card>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from "vue";
import {
  IonButton,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardSubtitle,
  IonCardTitle,
  IonInput,
  IonIcon,
  IonSelect,
  IonSelectOption,
  IonTextarea,
} from "@ionic/vue";

import type { Lote } from "@/models/LoteModel";
import type { Proveedor } from "@/models/proveedor";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";
import { micOffOutline, micOutline } from "ionicons/icons";
import { dictado_disponible, dictar_texto_largo } from "@/services/voz_service";
import { url_imagen_lote } from "@/services/lotes_service";

type CampoFormulario =
  | "nombre"
  | "descripcion"
  | "categoria"
  | "precioMercado"
  | "precioCupo"
  | "cantidadCupos"
  | "fechaInicio"
  | "fechaFin"
  | "proveedorId";

type ErroresFormulario = Partial<Record<CampoFormulario, string>>;

const props = withDefaults(
  defineProps<{
    lote?: Lote | null;
    proveedores?: Proveedor[];
    procesando?: boolean;
    error?: string | null;
  }>(),
  {
    lote: null,
    proveedores: () => [],
    procesando: false,
    error: null,
  },
);

const emit = defineEmits<{
  guardar: [payload: {
    request: CrearLoteRequest | ActualizarLoteRequest;
    imagen: File | null;
  }];
  cancelar: [];
}>();

const formulario = reactive({
  nombre: "",
  descripcion: "",
  categoria: "",
  precioMercado: "",
  precioCupo: "",
  cantidadCupos: "",
  fechaInicio: "",
  fechaFin: "",
  proveedorId: "",
});

const errores = reactive<ErroresFormulario>({});

const modo_edicion = computed(() => props.lote !== null);
const voz_disponible = ref(false);
const dictando = ref(false);
const error_voz = ref("");
const imagen_seleccionada = ref<File | null>(null);
const imagen_previa_seleccionada = ref<string | null>(null);
const error_imagen = ref("");
const imagen_previa = computed(() =>
  imagen_previa_seleccionada.value ??
  url_imagen_lote(props.lote?.imagenAvatar),
);

onMounted(async () => { voz_disponible.value = await dictado_disponible(); });

async function dictar_descripcion() {
  error_voz.value = "";
  dictando.value = true;
  try {
    const transcripcion = await dictar_texto_largo();
    formulario.descripcion = [formulario.descripcion.trim(), transcripcion].filter(Boolean).join(" ");
    limpiar_error_campo("descripcion");
  } catch (error) {
    error_voz.value = error instanceof Error ? error.message : "No se pudo iniciar el dictado. Podés escribir con el teclado.";
  } finally {
    dictando.value = false;
  }
}
const porcentaje_ahorro_calculado = computed(() => {
  const mercado = Number(formulario.precioMercado);
  const cupo = Number(formulario.precioCupo);
  if (!Number.isFinite(mercado) || !Number.isFinite(cupo) || mercado <= 0 || cupo < 0 || cupo >= mercado) return "—";
  return (((mercado - cupo) / mercado) * 100).toFixed(2);
});

function limpiar_formulario() {
  formulario.nombre = "";
  formulario.descripcion = "";
  formulario.categoria = "";
  formulario.precioMercado = "";
  formulario.precioCupo = "";
  formulario.cantidadCupos = "";
  formulario.fechaInicio = "";
  formulario.fechaFin = "";
  formulario.proveedorId = "";
  limpiar_imagen_seleccionada();

  limpiar_errores();
}

function limpiar_imagen_seleccionada() {
  if (imagen_previa_seleccionada.value) {
    URL.revokeObjectURL(imagen_previa_seleccionada.value);
  }
  imagen_previa_seleccionada.value = null;
  imagen_seleccionada.value = null;
  error_imagen.value = "";
}

function seleccionar_imagen(evento: Event) {
  const input = evento.target as HTMLInputElement;
  const archivo = input.files?.[0];
  if (!archivo) return;

  if (!["image/jpeg", "image/png", "image/webp"].includes(archivo.type)) {
    error_imagen.value = "Elegí una imagen JPG, PNG o WEBP.";
    input.value = "";
    return;
  }
  if (archivo.size > 5 * 1024 * 1024) {
    error_imagen.value = "La imagen no puede superar los 5 MB.";
    input.value = "";
    return;
  }

  limpiar_imagen_seleccionada();
  imagen_seleccionada.value = archivo;
  imagen_previa_seleccionada.value = URL.createObjectURL(archivo);
}

function cargar_lote() {
  limpiar_imagen_seleccionada();
  if (!props.lote) {
    limpiar_formulario();
    return;
  }

  formulario.nombre = props.lote.nombre;
  formulario.descripcion = props.lote.descripcion;
  formulario.categoria = props.lote.categoria;
  formulario.precioMercado = String(props.lote.precioMercado);
  formulario.precioCupo = String(props.lote.precioCupo);
  formulario.cantidadCupos = String(props.lote.cantidadCupos);
  formulario.fechaInicio = props.lote.fechaInicio.slice(0, 10);
  formulario.fechaFin = props.lote.fechaFin.slice(0, 10);
  formulario.proveedorId = String(props.lote.proveedorId);

  limpiar_errores();
}

watch(() => props.lote, cargar_lote, { immediate: true });

function limpiar_error_campo(campo: CampoFormulario) {
  delete errores[campo];
}

function limpiar_errores() {
  Object.keys(errores).forEach((campo) => {
    delete errores[campo as CampoFormulario];
  });
}

function obtener_numero(valor: string): number | null {
  if (valor.trim() === "") {
    return null;
  }

  const numero = Number(valor);

  if (!Number.isFinite(numero)) {
    return null;
  }

  return numero;
}

function es_fecha_valida(valor: string): boolean {
  if (!valor) {
    return false;
  }

  const fecha = new Date(`${valor}T00:00:00`);

  return !Number.isNaN(fecha.getTime());
}

function validar(): boolean {
  limpiar_errores();

  const precio_mercado = obtener_numero(formulario.precioMercado);
  const precio_cupo = obtener_numero(formulario.precioCupo);
  const cantidad_cupos = obtener_numero(formulario.cantidadCupos);
  const proveedor_id = obtener_numero(formulario.proveedorId);

  if (!formulario.nombre.trim()) {
    errores.nombre = "El nombre es obligatorio.";
  }

  if (!formulario.descripcion.trim()) {
    errores.descripcion = "La descripción es obligatoria.";
  }

  if (!formulario.categoria.trim()) {
    errores.categoria = "La categoría es obligatoria.";
  }

  if (precio_mercado === null) {
    errores.precioMercado = "Ingresá un precio de mercado válido.";
  } else if (precio_mercado <= 0) {
    errores.precioMercado = "El precio de mercado debe ser mayor a 0.";
  }

  if (precio_cupo === null) {
    errores.precioCupo = "Ingresá un precio de cupo válido.";
  } else if (precio_cupo <= 0) {
    errores.precioCupo = "El precio de cupo debe ser mayor a 0.";
  }

  if (
    precio_mercado !== null &&
    precio_cupo !== null &&
    precio_cupo >= precio_mercado
  ) {
    errores.precioCupo =
      "El precio de cupo debe ser menor al precio de mercado.";
  }
  if (cantidad_cupos === null) {
    errores.cantidadCupos = "Ingresá una cantidad de cupos.";
  } else if (!Number.isInteger(cantidad_cupos) || cantidad_cupos <= 0) {
    errores.cantidadCupos =
      "La cantidad de cupos debe ser un número entero mayor a 0.";
  } else if (props.lote && cantidad_cupos < props.lote.cuposOcupados) {
    errores.cantidadCupos = `La cantidad de cupos no puede ser menor a los ${props.lote.cuposOcupados} cupos ya ocupados.`;
  }

  if (!formulario.fechaInicio) {
    errores.fechaInicio = "La fecha de inicio es obligatoria.";
  } else if (!es_fecha_valida(formulario.fechaInicio)) {
    errores.fechaInicio = "La fecha de inicio no es válida.";
  }

  if (!formulario.fechaFin) {
    errores.fechaFin = "La fecha de finalización es obligatoria.";
  } else if (!es_fecha_valida(formulario.fechaFin)) {
    errores.fechaFin = "La fecha de finalización no es válida.";
  }

  if (
    es_fecha_valida(formulario.fechaInicio) &&
    es_fecha_valida(formulario.fechaFin) &&
    formulario.fechaFin <= formulario.fechaInicio
  ) {
    errores.fechaFin =
      "La fecha de finalización debe ser posterior a la fecha de inicio.";
  }

  if (!formulario.proveedorId) {
    errores.proveedorId = "Seleccioná un proveedor.";
  } else if (proveedor_id === null || proveedor_id <= 0) {
    errores.proveedorId = "El proveedor seleccionado no es válido.";
  }

  return Object.keys(errores).length === 0;
}

function guardar() {
  if (!validar()) {
    return;
  }

  const precio_mercado = obtener_numero(formulario.precioMercado);
  const precio_cupo = obtener_numero(formulario.precioCupo);
  const cantidad_cupos = obtener_numero(formulario.cantidadCupos);
  const proveedor_id = obtener_numero(formulario.proveedorId);

  if (
    precio_mercado === null ||
    precio_cupo === null ||
    cantidad_cupos === null ||
    proveedor_id === null
  ) {
    return;
  }

  emit("guardar", {
    request: {
      nombre: formulario.nombre.trim(),
      descripcion: formulario.descripcion.trim(),
      categoria: formulario.categoria.trim(),
      precioMercado: precio_mercado,
      precioCupo: precio_cupo,
      cantidadCupos: cantidad_cupos,
      fechaInicio: formulario.fechaInicio,
      fechaFin: formulario.fechaFin,
      proveedorId: proveedor_id,
      imagenAvatar: props.lote?.imagenAvatar ?? null,
    },
    imagen: imagen_seleccionada.value,
  });
}

function cancelar() {
  emit("cancelar");
}

onBeforeUnmount(limpiar_imagen_seleccionada);
</script>

<style scoped>
.syner-lote-formulario {
  width: 100%;
  max-width: 900px;
  margin: 16px auto;
}

.syner-lote-formulario ion-card-header {
  padding-bottom: 8px;
}

.syner-lote-formulario ion-card-title {
  --color: var(--syner-text);
  color: var(--syner-text);
}

.syner-lote-formulario ion-card-subtitle {
  --color: var(--syner-text-secondary);
  color: var(--syner-text-secondary);
}

.syner-lote-formulario ion-input,
.syner-lote-formulario ion-textarea,
.syner-lote-formulario ion-select {
  --background: var(--syner-surface);
  --color: var(--syner-text);
  --placeholder-color: var(--syner-text-secondary);
  --label-color: var(--syner-text-secondary);
  --highlight-color-focused: var(--syner-primary);
  --highlight-color-valid: var(--syner-success);
  --highlight-color-invalid: var(--ion-color-danger);
  --border-color: var(--syner-border);
}

.syner-lote-formulario__hint {
  margin: 8px 0 16px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-lote-formulario__voz { display: flex; align-items: center; flex-wrap: wrap; gap: 8px; margin-top: 8px; }
.syner-lote-formulario__voz span { color: var(--syner-text-secondary); font-size: 13px; }
.syner-lote-formulario__image-label { display: grid; gap: 4px; color: var(--syner-text); font-size: 14px; font-weight: 650; }
.syner-lote-formulario__image-label span { color: var(--syner-text-secondary); font-size: 12px; font-weight: 400; }
.syner-lote-formulario__image-input { width: 100%; margin-top: 9px; padding: 11px; border: 1px dashed var(--syner-border); border-radius: 12px; background: var(--syner-surface); color: var(--syner-text-secondary); }
.syner-lote-formulario__image-preview { display: flex; align-items: center; flex-wrap: wrap; gap: 12px; margin-top: 10px; }
.syner-lote-formulario__image-preview img { width: min(100%, 240px); height: 150px; border-radius: 14px; object-fit: cover; background: var(--syner-background); }
.syner-lote-formulario__image-preview span { color: var(--syner-text-secondary); font-size: 13px; }

.syner-lote-formulario__ahorro {
  display: flex;
  align-items: center;
  justify-content: space-between;
  align-self: end;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 12px;
  background: color-mix(in srgb, var(--syner-success) 10%, transparent);
  color: var(--syner-success);
}

.syner-lote-formulario__ahorro strong { font-size: 18px; }

.syner-lote-formulario__grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.syner-lote-formulario__field {
  min-width: 0;
}

.syner-lote-formulario__full {
  grid-column: 1 / -1;
}

.syner-lote-formulario__field-error {
  display: block;
  margin: 6px 4px 0;
  color: var(--ion-color-danger);
  font-size: 13px;
  line-height: 1.35;
}

.syner-lote-formulario__error {
  margin: 20px 0 0;
  color: var(--ion-color-danger);
  font-size: 14px;
}

.syner-lote-formulario__actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

@media (max-width: 768px) {
  .syner-lote-formulario {
    margin: 12px 0;
  }

  .syner-lote-formulario__grid {
    grid-template-columns: 1fr;
  }

  .syner-lote-formulario__full {
    grid-column: auto;
  }

  .syner-lote-formulario__actions {
    justify-content: stretch;
  }

  .syner-lote-formulario__actions ion-button {
    flex: 1;
  }
}
</style>
```
