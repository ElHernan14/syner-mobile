<template>
  <comp-page titulo="Mi ubicación" :mostrar_volver="es_ubicacion_proveedor" ruta_volver="/app/proveedores">
    <div class="syner-ubicacion">
      <header class="syner-ubicacion__header">
        <div class="syner-ubicacion__icon"><ion-icon :icon="locationOutline" /></div>
        <div>
          <p class="syner-ubicacion__eyebrow">{{ es_ubicacion_proveedor ? "Ubicación del proveedor" : "Tu punto en el mapa" }}</p>
          <h1>{{ es_ubicacion_proveedor ? nombre_proveedor : "Mi ubicación" }}</h1>
          <p>{{ es_ubicacion_proveedor ? "La ubicación registrada de este proveedor." : "Buscá una dirección o usá la posición actual del teléfono." }}</p>
        </div>
      </header>

      <form class="syner-ubicacion__busqueda" @submit.prevent="buscar_direccion">
        <ion-input v-model="direccion_busqueda" label="Buscar dirección" label-placement="stacked" fill="outline" placeholder="Calle, ciudad o provincia" :disabled="buscando" />
        <ion-button type="submit" :disabled="buscando || !direccion_busqueda.trim()"><ion-icon slot="start" :icon="searchOutline" />{{ buscando ? "Buscando…" : "Buscar" }}</ion-button>
      </form>

      <comp-mapa-osm v-if="mapa_visible" :latitud="centro.latitud" :longitud="centro.longitud" @mover="mover_centro" />
      <comp-estado-listado v-else tipo="vacio" titulo="Ubicación todavía no cargada" mensaje="Este proveedor no tiene coordenadas registradas. Un administrador puede cargarlas en la base para mostrar su punto en el mapa." />

      <div v-if="!es_ubicacion_proveedor || mapa_visible" class="syner-ubicacion__acciones">
        <ion-button v-if="!es_ubicacion_proveedor" fill="outline" @click="usar_ubicacion_actual" :disabled="cargando_gps">
          <ion-spinner v-if="cargando_gps" slot="start" name="crescent" /><ion-icon v-else slot="start" :icon="locateOutline" />{{ cargando_gps ? "Localizando…" : "Usar mi ubicación" }}
        </ion-button>
        <ion-button fill="outline" @click="confirmar_punto" :disabled="buscando || !mapa_visible">{{ es_ubicacion_proveedor ? "Actualizar dirección del punto" : "Confirmar punto" }}</ion-button>
      </div>

      <div v-if="ubicacion_confirmada" class="syner-ubicacion__resultado">
        <div><span>Dirección</span><strong>{{ ubicacion_confirmada.direccion }}</strong></div>
        <div class="syner-ubicacion__coordenadas"><span>Latitud</span><strong>{{ ubicacion_confirmada.latitud.toFixed(6) }}</strong><span>Longitud</span><strong>{{ ubicacion_confirmada.longitud.toFixed(6) }}</strong></div>
      </div>
      <p v-if="mensaje" class="syner-ubicacion__mensaje" :class="{ 'syner-ubicacion__mensaje--error': es_error }" role="status">{{ mensaje }}</p>
      <p class="syner-ubicacion__atribucion">El mapa usa OpenStreetMap. Arrastrá el mapa o pellizcá para mover y acercar el punto.</p>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { IonButton, IonIcon, IonInput, IonSpinner } from "@ionic/vue";
import { locateOutline, locationOutline, searchOutline } from "ionicons/icons";
import CompPage from "@/components/shared/comp_page.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import CompMapaOsm from "@/components/ubicacion/comp_mapa_osm.vue";
import { obtener_ubicacion_actual } from "@/services/geolocalizacion_service";

interface Punto { latitud: number; longitud: number }
interface UbicacionConfirmada extends Punto { direccion: string }
interface ResultadoNominatim { lat: string; lon: string; display_name?: string }
const route = useRoute();
const centro = ref<Punto>({ latitud: -33.3017, longitud: -66.3378 });
const es_ubicacion_proveedor = computed(() => route.query.proveedor === "true");
const nombre_proveedor = computed(() => String(route.query.nombre ?? "Proveedor"));
const mapa_visible = ref(!es_ubicacion_proveedor.value);
const direccion_busqueda = ref("");
const ubicacion_confirmada = ref<UbicacionConfirmada | null>(null);
const buscando = ref(false);
const cargando_gps = ref(false);
const mensaje = ref("");
const es_error = ref(false);

watch(() => route.query, (query) => {
  const latitud = Number(query.lat);
  const longitud = Number(query.lon);
  const es_proveedor = query.proveedor === "true";
  if (es_proveedor && Number.isFinite(latitud) && Number.isFinite(longitud) && Math.abs(latitud) <= 90 && Math.abs(longitud) <= 180) {
    centro.value = { latitud, longitud };
    mapa_visible.value = true;
    ubicacion_confirmada.value = null;
  } else if (es_proveedor) {
    mapa_visible.value = false;
  } else {
    mapa_visible.value = true;
  }
  direccion_busqueda.value = "";
  mensaje.value = "";
}, { immediate: true });

function mover_centro(punto: Punto) { centro.value = punto; ubicacion_confirmada.value = null; }

async function buscar_direccion() {
  const termino = direccion_busqueda.value.trim();
  if (!termino) return;
  buscando.value = true; mensaje.value = ""; es_error.value = false;
  try {
    const parametros = new URLSearchParams({ q: termino, format: "jsonv2", limit: "1", "accept-language": "es" });
    const response = await fetch(`https://nominatim.openstreetmap.org/search?${parametros}`);
    if (!response.ok) throw new Error("No pudimos buscar esa dirección.");
    const resultados = await response.json() as ResultadoNominatim[];
    if (!resultados.length) throw new Error("No encontramos esa dirección. Podés mover el mapa y elegir el punto.");
    const punto = resultados[0];
    centro.value = { latitud: Number(punto.lat), longitud: Number(punto.lon) };
    mapa_visible.value = true;
    ubicacion_confirmada.value = null;
    mensaje.value = "Resultado ubicado en el mapa. Mové el punto si hace falta y confirmalo.";
  } catch (error) { mensaje.value = error instanceof Error ? error.message : "No se pudo consultar la dirección. Podés seguir eligiendo el punto en el mapa."; es_error.value = true; }
  finally { buscando.value = false; }
}

async function confirmar_punto() {
  buscando.value = true; mensaje.value = ""; es_error.value = false;
  const { latitud, longitud } = centro.value;
  try {
    const parametros = new URLSearchParams({ lat: String(latitud), lon: String(longitud), format: "jsonv2", "accept-language": "es" });
    const response = await fetch(`https://nominatim.openstreetmap.org/reverse?${parametros}`);
    if (!response.ok) throw new Error("No se pudo consultar la dirección de este punto.");
    const resultado = await response.json() as { display_name?: string };
    ubicacion_confirmada.value = { latitud, longitud, direccion: resultado.display_name ?? "Dirección sin nombre" };
    mensaje.value = es_ubicacion_proveedor.value ? "Punto del proveedor confirmado." : "Punto confirmado para esta consulta.";
  } catch (error) {
    ubicacion_confirmada.value = { latitud, longitud, direccion: direccion_busqueda.value.trim() || "Dirección no disponible; se conservan las coordenadas." };
    mensaje.value = error instanceof Error ? `${error.message} Las coordenadas del punto siguen disponibles.` : "La dirección no está disponible; se conservan las coordenadas.";
    es_error.value = true;
  } finally { buscando.value = false; }
}

async function usar_ubicacion_actual() {
  cargando_gps.value = true; mensaje.value = ""; es_error.value = false;
  try {
    const position = await obtener_ubicacion_actual();
    centro.value = { latitud: position.coords.latitude, longitud: position.coords.longitude };
    mapa_visible.value = true;
    ubicacion_confirmada.value = null;
  } catch (error) { mensaje.value = error instanceof Error ? error.message : "No se pudo obtener la ubicación. Podés buscar una dirección o mover el mapa."; es_error.value = true; }
  finally { cargando_gps.value = false; }
}
</script>

<style scoped>
.syner-ubicacion { width: min(100% - 32px, 1040px); margin: 0 auto; padding: 24px 0 40px; color: var(--syner-text); }
.syner-ubicacion__header { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
.syner-ubicacion__icon { flex: 0 0 58px; width: 58px; height: 58px; display: grid; place-items: center; border-radius: 18px; background: color-mix(in srgb, var(--syner-primary) 14%, transparent); color: var(--syner-primary); font-size: 30px; }
.syner-ubicacion__eyebrow { margin: 0 0 4px !important; color: var(--syner-primary) !important; font-size: 12px; font-weight: 800; letter-spacing: .08em; text-transform: uppercase; }
.syner-ubicacion h1 { margin: 0; color: var(--syner-text); font-size: clamp(24px, 5vw, 34px); }
.syner-ubicacion__header p:last-child { margin: 6px 0 0; color: var(--syner-text-secondary); }
.syner-ubicacion__busqueda { display: grid; grid-template-columns: minmax(0,1fr) auto; align-items: end; gap: 10px; margin: 18px 0; }
.syner-ubicacion__busqueda ion-input { --background: var(--syner-surface); --color: var(--syner-text); --label-color: var(--syner-text-secondary); --border-color: var(--syner-border); }
.syner-ubicacion__acciones { display: flex; justify-content: flex-end; flex-wrap: wrap; gap: 10px; margin-top: 14px; }
.syner-ubicacion__resultado { display: grid; gap: 12px; margin-top: 16px; padding: 16px; border: 1px solid var(--syner-border); border-radius: 16px; background: var(--syner-surface); }
.syner-ubicacion__resultado div { display: grid; gap: 4px; }
.syner-ubicacion__resultado span { color: var(--syner-text-secondary); font-size: 13px; }
.syner-ubicacion__resultado strong { color: var(--syner-text); overflow-wrap: anywhere; }
.syner-ubicacion__coordenadas { grid-template-columns: auto 1fr auto 1fr !important; align-items: center; gap: 8px 12px !important; }
.syner-ubicacion__mensaje { margin: 14px 2px 0; color: var(--syner-success); }
.syner-ubicacion__mensaje--error { color: var(--ion-color-danger); }
.syner-ubicacion__atribucion { margin: 12px 2px 0; color: var(--syner-text-secondary); font-size: 12px; }
@media (max-width: 560px) { .syner-ubicacion__busqueda { grid-template-columns: 1fr; } .syner-ubicacion__busqueda ion-button { width: 100%; } .syner-ubicacion__acciones ion-button { flex: 1 1 100%; } .syner-ubicacion__coordenadas { grid-template-columns: auto 1fr !important; } }
</style>
