<template>
  <section ref="contenedor" class="mapa-osm" :class="{ 'mapa-osm--expandido': expandido }" :aria-label="mapa_satellite ? 'Mapa satelital interactivo' : 'Mapa interactivo OpenStreetMap'" @pointerdown="iniciar_gesto" @pointermove="mover_gesto" @pointerup="finalizar_gesto" @pointercancel="finalizar_gesto" @wheel.prevent="zoom_rueda">
    <div class="mapa-osm__tiles" aria-hidden="true">
      <img v-for="tile in tiles" :key="tile.key" :src="tile.url" alt="" draggable="false" :style="{ left: `${tile.left}px`, top: `${tile.top}px` }" @error="tile_cargada = false" @load="tile_cargada = true" />
    </div>
    <div v-if="!props.marcadorFijo" class="mapa-osm__reticula" aria-hidden="true"><span /></div>
    <div
      v-if="!props.marcadorFijo || marca_visible"
      class="mapa-osm__marca"
      :class="{ 'mapa-osm__marca--fija': props.marcadorFijo }"
      :style="props.marcadorFijo ? estilo_marca : undefined"
      aria-hidden="true"
    >
      <ion-icon :icon="location" />
    </div>
    <div class="mapa-osm__controles" @pointerdown.stop>
      <ion-button fill="solid" size="small" :aria-pressed="mapa_satellite" @click="mapa_satellite = !mapa_satellite">
        {{ mapa_satellite ? "Mapa" : "Satélite" }}
      </ion-button>
      <ion-button fill="solid" size="small" :aria-label="expandido ? 'Reducir mapa' : 'Ampliar mapa'" @click="expandido = !expandido">
        {{ expandido ? "Reducir" : "Ampliar" }}
      </ion-button>
    </div>
    <div class="mapa-osm__zoom" @pointerdown.stop>
      <ion-button fill="solid" size="small" aria-label="Acercar mapa" @click="ajustar_zoom(1)">+</ion-button>
      <ion-button fill="solid" size="small" aria-label="Alejar mapa" @click="ajustar_zoom(-1)">−</ion-button>
    </div>
    <span v-if="!tile_cargada" class="mapa-osm__aviso">El mapa no está disponible ahora. Podés continuar con las coordenadas o la dirección.</span>
    <a v-if="mapa_satellite" class="mapa-osm__atribucion" href="https://www.esri.com/en-us/legal/terms/full-master-agreement" target="_blank" rel="noreferrer" @pointerdown.stop>Imagery © Esri</a>
    <a v-else class="mapa-osm__atribucion" href="https://www.openstreetmap.org/copyright" target="_blank" rel="noreferrer" @pointerdown.stop>© OpenStreetMap contributors</a>
  </section>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";
import { IonButton, IonIcon } from "@ionic/vue";
import { location } from "ionicons/icons";

const props = withDefaults(defineProps<{ latitud: number; longitud: number; zoom?: number; marcadorFijo?: boolean }>(), { zoom: 15, marcadorFijo: false });
const emit = defineEmits<{ mover: [punto: { latitud: number; longitud: number }] }>();
const TAMANO_TILE = 256;
const contenedor = ref<HTMLElement | null>(null);
const ancho = ref(360);
const alto = ref(420);
const zoom = ref(props.zoom);
const centro = ref({ latitud: props.latitud, longitud: props.longitud });
const tile_cargada = ref(true);
const mapa_satellite = ref(false);
const expandido = ref(false);
const punteros = new Map<number, { x: number; y: number }>();
let gesto: { centro: { latitud: number; longitud: number }; zoom: number; punteros: { x: number; y: number }[]; distancia: number } | null = null;
let observador: ResizeObserver | null = null;

function pixeles_mundo(longitud: number, latitud: number, nivel = zoom.value) {
  const escala = TAMANO_TILE * 2 ** nivel;
  const lat = Math.max(-85.05112878, Math.min(85.05112878, latitud)) * Math.PI / 180;
  return { x: (longitud + 180) / 360 * escala, y: (1 - Math.asinh(Math.tan(lat)) / Math.PI) / 2 * escala };
}
function coordenadas_mundo(x: number, y: number, nivel = zoom.value) {
  const escala = TAMANO_TILE * 2 ** nivel;
  const longitud = x / escala * 360 - 180;
  const n = Math.PI - 2 * Math.PI * y / escala;
  const latitud = 180 / Math.PI * Math.atan(Math.sinh(n));
  return { latitud, longitud: ((longitud + 540) % 360) - 180 };
}

const tiles = computed(() => {
  const nivel = zoom.value;
  const dimension = 2 ** nivel;
  const mundo = pixeles_mundo(centro.value.longitud, centro.value.latitud, nivel);
  const origenX = mundo.x - ancho.value / 2;
  const origenY = mundo.y - alto.value / 2;
  const inicioX = Math.floor(origenX / TAMANO_TILE) - 1;
  const inicioY = Math.floor(origenY / TAMANO_TILE) - 1;
  const columnas = Math.ceil(ancho.value / TAMANO_TILE) + 3;
  const filas = Math.ceil(alto.value / TAMANO_TILE) + 3;
  const salida: { key: string; url: string; left: number; top: number }[] = [];
  for (let fila = 0; fila < filas; fila++) for (let columna = 0; columna < columnas; columna++) {
    const x = inicioX + columna;
    const y = inicioY + fila;
    if (y < 0 || y >= dimension) continue;
    const xEnvuelto = ((x % dimension) + dimension) % dimension;
    salida.push({
      key: `${mapa_satellite.value ? "sat" : "osm"}/${nivel}/${x}/${y}`,
      url: mapa_satellite.value
        ? `https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/${nivel}/${y}/${xEnvuelto}`
        : `https://tile.openstreetmap.org/${nivel}/${xEnvuelto}/${y}.png`,
      left: x * TAMANO_TILE - origenX,
      top: y * TAMANO_TILE - origenY,
    });
  }
  return salida;
});

const posicion_marca = computed(() => {
  const nivel = zoom.value;
  const mundo_centro = pixeles_mundo(centro.value.longitud, centro.value.latitud, nivel);
  const origen_x = mundo_centro.x - ancho.value / 2;
  const origen_y = mundo_centro.y - alto.value / 2;
  const mundo_marca = pixeles_mundo(props.longitud, props.latitud, nivel);
  const ancho_mundo = TAMANO_TILE * 2 ** nivel;
  let x = mundo_marca.x - origen_x;
  if (x < -ancho_mundo / 2) x += ancho_mundo;
  else if (x > ancho_mundo * 1.5) x -= ancho_mundo;
  return { x, y: mundo_marca.y - origen_y };
});
const marca_visible = computed(() =>
  posicion_marca.value.x >= -48 &&
  posicion_marca.value.x <= ancho.value + 48 &&
  posicion_marca.value.y >= -52 &&
  posicion_marca.value.y <= alto.value + 10,
);
const estilo_marca = computed(() => ({
  left: `${posicion_marca.value.x}px`,
  top: `${posicion_marca.value.y}px`,
}));

function emitir_centro() { emit("mover", { ...centro.value }); }
function ajustar_zoom(cambio: number) { zoom.value = Math.max(3, Math.min(19, zoom.value + cambio)); tile_cargada.value = true; }
function distancia(a: { x: number; y: number }, b: { x: number; y: number }) { return Math.hypot(a.x - b.x, a.y - b.y); }
function iniciar_gesto(evento: PointerEvent) {
  if ((evento.target as HTMLElement).closest("button, a")) return;
  contenedor.value?.setPointerCapture(evento.pointerId);
  punteros.set(evento.pointerId, { x: evento.clientX, y: evento.clientY });
  const puntos = [...punteros.values()];
  gesto = { centro: { ...centro.value }, zoom: zoom.value, punteros: puntos, distancia: puntos.length > 1 ? distancia(puntos[0], puntos[1]) : 0 };
}
function mover_gesto(evento: PointerEvent) {
  if (!punteros.has(evento.pointerId) || !gesto || !contenedor.value) return;
  punteros.set(evento.pointerId, { x: evento.clientX, y: evento.clientY });
  const puntos = [...punteros.values()];
  if (puntos.length > 1 && gesto.punteros.length > 1 && gesto.distancia > 0) {
    const ratio = distancia(puntos[0], puntos[1]) / gesto.distancia;
    zoom.value = Math.max(3, Math.min(19, Math.round(gesto.zoom + Math.log2(ratio))));
  }
  const origen = pixeles_mundo(gesto.centro.longitud, gesto.centro.latitud, zoom.value);
  const desplazamientoX = puntos[0].x - gesto.punteros[0].x;
  const desplazamientoY = puntos[0].y - gesto.punteros[0].y;
  centro.value = coordenadas_mundo(origen.x - desplazamientoX, origen.y - desplazamientoY, zoom.value);
  if (!props.marcadorFijo) emitir_centro();
}
function finalizar_gesto(evento: PointerEvent) {
  punteros.delete(evento.pointerId);
  if (punteros.size === 0) gesto = null;
  else {
    const puntos = [...punteros.values()];
    gesto = { centro: { ...centro.value }, zoom: zoom.value, punteros: puntos, distancia: puntos.length > 1 ? distancia(puntos[0], puntos[1]) : 0 };
  }
}
function zoom_rueda(evento: WheelEvent) { ajustar_zoom(evento.deltaY < 0 ? 1 : -1); }
watch(() => [props.latitud, props.longitud] as const, ([latitud, longitud]) => { if (Number.isFinite(latitud) && Number.isFinite(longitud)) centro.value = { latitud, longitud }; });
onMounted(() => {
  if (!contenedor.value) return;
  observador = new ResizeObserver(([entrada]) => { ancho.value = entrada.contentRect.width; alto.value = entrada.contentRect.height; });
  observador.observe(contenedor.value);
});
onBeforeUnmount(() => observador?.disconnect());
</script>

<style scoped>
.mapa-osm { position: relative; width: 100%; height: clamp(300px, 48vh, 540px); overflow: hidden; border: 1px solid var(--syner-border); border-radius: 22px; background: #e9edf1; touch-action: none; cursor: grab; }
.mapa-osm--expandido { position: fixed; z-index: 10000; inset: 12px; width: auto; height: auto; max-height: none; border-radius: 18px; box-shadow: 0 0 0 100vmax #0008; }
.mapa-osm:active { cursor: grabbing; }
.mapa-osm__tiles { position: absolute; inset: 0; }
.mapa-osm__tiles img { position: absolute; width: 256px; height: 256px; max-width: none; user-select: none; pointer-events: none; }
.mapa-osm__reticula { position: absolute; inset: 50% auto auto 50%; width: 30px; height: 30px; transform: translate(-50%, -50%); border: 2px solid #fff; border-radius: 50%; box-shadow: 0 1px 8px #0008; pointer-events: none; }
.mapa-osm__reticula span { position: absolute; inset: 50% auto auto 50%; width: 8px; height: 8px; border-radius: 50%; background: var(--syner-primary); transform: translate(-50%, -50%); }
.mapa-osm__marca { position: absolute; left: 50%; top: 50%; transform: translate(-50%, -100%); color: #e33a55; font-size: 42px; filter: drop-shadow(0 2px 2px #0007); pointer-events: none; }
.mapa-osm__marca--fija { transform: translate(-50%, -100%); }
.mapa-osm__controles { position: absolute; top: 12px; left: 12px; display: flex; flex-wrap: wrap; gap: 6px; }
.mapa-osm__zoom { position: absolute; top: 12px; right: 12px; display: grid; gap: 6px; }
.mapa-osm__zoom ion-button { margin: 0; font-size: 20px; }
.mapa-osm__atribucion { position: absolute; right: 4px; bottom: 3px; padding: 2px 5px; border-radius: 4px; background: #ffffffd9; color: #225785; font-size: 11px; text-decoration: none; touch-action: auto; }
.mapa-osm__aviso { position: absolute; left: 12px; right: 58px; top: 12px; padding: 8px 10px; border-radius: 10px; background: var(--syner-surface); color: var(--syner-text); font-size: 12px; }
</style>
