<template>
  <ion-page>
    <ion-content class="publico">
      <main>
        <header>
          <span>SYNER · CATÁLOGO PÚBLICO</span>
          <h1>Encontrá tu próximo cupo</h1>
          <p>Lotes disponibles de proveedores de la comunidad.</p>
        </header>

        <p v-if="cargando">Cargando catálogo…</p>
        <p v-else-if="error && !lotes.length" role="alert">{{ error }}</p>
        <p v-if="usando_cache" class="aviso-cache">
          Sin conexión: se muestran los últimos lotes guardados.
        </p>

        <section v-if="!cargando && lotes.length" class="grid">
          <article v-for="lote in lotes" :key="lote.nombre + lote.fecha_fin">
            <img
              :src="imagen_lote(lote)"
              :alt="`Imagen del lote ${lote.nombre}`"
              loading="lazy"
              @error="usar_imagen_respaldo"
            />
            <small>
              {{ lote.categoria }} · {{ lote.proveedor }}
              <b v-if="lote.proveedor_verificado">✓ Verificado</b>
            </small>
            <h2>{{ lote.nombre }}</h2>
            <p>{{ lote.descripcion }}</p>
            <strong>{{ moneda(lote.precio_cupo) }}</strong>
            <s>{{ moneda(lote.precio_mercado) }}</s>
            <span class="ahorro">{{ lote.porcentaje_ahorro }}% de ahorro</span>
            <footer>
              {{ lote.cupos_disponibles }} cupos · Finaliza
              {{ new Date(lote.fecha_fin).toLocaleDateString("es-AR") }}
            </footer>
          </article>
        </section>

        <p v-else-if="!cargando && !error && !lotes.length">
          Todavía no hay lotes disponibles.
        </p>
        <a href="https://syner-api.runasp.net/privacidad.html">
          Política de privacidad
        </a>
      </main>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { IonContent, IonPage } from "@ionic/vue";
import { obtener } from "@/services/ajax_service";
import { url_imagen_lote } from "@/services/lotes_service";
import { hay_conexion } from "@/services/conectividad_service";

interface LotePublico {
  nombre: string;
  descripcion: string;
  categoria: string;
  precio_cupo: number;
  precio_mercado: number;
  porcentaje_ahorro: number;
  cupos_disponibles: number;
  fecha_fin: string;
  proveedor: string;
  proveedor_verificado: boolean;
  imagen_avatar?: string | null;
}

const CACHE_CATALOGO = "syner_catalogo_publico_cache_v1";
const IMAGEN_LOTE_DEFAULT = "/assets/lote-default.webp";
const lotes = ref<LotePublico[]>([]);
const cargando = ref(true);
const error = ref("");
const usando_cache = ref(false);

const moneda = (valor: number) =>
  new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS",
    maximumFractionDigits: 0,
  }).format(valor);

function leer_cache(): LotePublico[] {
  try {
    const cache: unknown = JSON.parse(localStorage.getItem(CACHE_CATALOGO) ?? "null");
    if (!Array.isArray(cache) || !cache.every(es_lote_publico)) return [];
    return cache;
  } catch (error) {
    console.warn("No se pudo leer el caché del catálogo público.", error);
    return [];
  }
}

function es_lote_publico(valor: unknown): valor is LotePublico {
  if (typeof valor !== "object" || valor === null) return false;
  return "nombre" in valor && typeof valor.nombre === "string" &&
    "descripcion" in valor && typeof valor.descripcion === "string" &&
    "categoria" in valor && typeof valor.categoria === "string" &&
    "precio_cupo" in valor && typeof valor.precio_cupo === "number" &&
    "precio_mercado" in valor && typeof valor.precio_mercado === "number" &&
    "porcentaje_ahorro" in valor && typeof valor.porcentaje_ahorro === "number" &&
    "cupos_disponibles" in valor && typeof valor.cupos_disponibles === "number" &&
    "fecha_fin" in valor && typeof valor.fecha_fin === "string" &&
    "proveedor" in valor && typeof valor.proveedor === "string" &&
    "proveedor_verificado" in valor && typeof valor.proveedor_verificado === "boolean" &&
    (!("imagen_avatar" in valor) || valor.imagen_avatar === null || typeof valor.imagen_avatar === "string");
}

function imagen_lote(lote: LotePublico): string {
  return url_imagen_lote(lote.imagen_avatar) ?? IMAGEN_LOTE_DEFAULT;
}

function usar_imagen_respaldo(evento: Event) {
  const imagen = evento.currentTarget;
  if (imagen instanceof HTMLImageElement && !imagen.src.endsWith(IMAGEN_LOTE_DEFAULT)) {
    imagen.src = IMAGEN_LOTE_DEFAULT;
  }
}

async function cargar_catalogo() {
  const cache = leer_cache();
  if (!(await hay_conexion())) {
    lotes.value = cache;
    usando_cache.value = cache.length > 0;
    error.value = cache.length ? "" : "No hay conexión ni un catálogo guardado en este dispositivo.";
    cargando.value = false;
    return;
  }

  try {
    const resultado = await obtener<LotePublico[]>("publico/lotes");
    if (!Array.isArray(resultado)) {
      throw new Error("La API devolvió un catálogo con un formato inválido.");
    }
    lotes.value = resultado;
    try {
      localStorage.setItem(CACHE_CATALOGO, JSON.stringify(resultado));
    } catch (error) {
      console.warn("No se pudo guardar el caché del catálogo público.", error);
    }
  } catch (err) {
    lotes.value = cache;
    usando_cache.value = cache.length > 0;
    error.value = cache.length
      ? ""
      : err instanceof Error
        ? err.message
        : "No pudimos cargar el catálogo. Probá nuevamente más tarde.";
  } finally {
    cargando.value = false;
  }
}

onMounted(() => void cargar_catalogo());
</script>

<style scoped>
.publico { --background: #f4f7fb; color: #172033; }
.publico main { max-width: 1050px; margin: auto; padding: 32px 20px; }
.publico header { padding: 28px; border-radius: 24px; background: linear-gradient(135deg,#123a78,#2877d4); color: white; }
.publico header span { font-size: 12px; letter-spacing: .14em; }
.publico h1 { font-size: clamp(28px,6vw,46px); margin: 10px 0; }
.publico .grid { display: grid; grid-template-columns: repeat(auto-fit,minmax(270px,1fr)); gap: 16px; margin: 24px 0; }
.publico article { display: flex; flex-direction: column; gap: 10px; padding: 20px; border-radius: 20px; background: white; box-shadow: 0 12px 32px #15335a13; }
.publico article img { width: calc(100% + 40px); max-width: none; height: 190px; margin: -20px -20px 4px; object-fit: cover; border-radius: 20px 20px 0 0; background: #e9eef5; }
.publico small, .publico footer { color: #66758c; }
.publico article h2, .publico article p { margin: 0; }
.publico article strong { font-size: 24px; }
.publico article s { color: #78859a; }
.publico article b, .ahorro { color: #14845d; }
.publico footer { margin-top: auto; font-size: 13px; }
.aviso-cache { padding: 10px 12px; border-radius: 12px; background: #fef3c7; color: #172033; }
</style>
