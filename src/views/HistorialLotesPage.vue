<template>
  <comp-page titulo="Historial de lotes" :mostrar_refresher="true" @actualizar="cargar">
    <div class="syner-historial">
      <header><span class="syner-historial__icon"><ion-icon :icon="timeOutline" /></span><div><p>Auditoría · solo lectura</p><h1>Movimientos de estado</h1><span>Quién modificó cada lote y cuándo.</span></div></header>
      <div v-if="cargando" class="syner-historial__loading"><comp-esqueleto :filas="4" /></div>
      <comp-estado-listado v-else-if="error && !items.length" tipo="error" titulo="No se pudo cargar el historial" :mensaje="error" reintentar @reintentar="cargar" />
      <comp-estado-listado v-else-if="!items.length" tipo="vacio" titulo="Todavía no hay cambios registrados" mensaje="Cada cambio de estado de un lote aparecerá aquí." />
      <div v-else class="syner-historial__lista">
        <article v-for="item in items" :key="item.id" class="syner-historial__card">
          <div class="syner-historial__marca"><ion-icon :icon="swapHorizontalOutline" /></div>
          <div class="syner-historial__contenido">
            <div class="syner-historial__top"><span>Lote #{{ item.lote_id }}</span><time>{{ fecha(item.fecha_modificacion) }}</time></div>
            <h2>{{ item.lote }}</h2>
            <div class="syner-historial__transicion"><comp-estado-badge :estado="item.estado_anterior" /><ion-icon :icon="arrowForwardOutline" /><comp-estado-badge :estado="item.nuevo_estado" /></div>
            <p>Modificado por <strong>{{ item.usuario }}</strong></p>
          </div>
        </article>
      </div>
      <comp-paginacion v-if="total_paginas > 1" :pagina="pagina" :total_paginas="total_paginas" @cambiar="cambiar_pagina" />
      <p v-if="usando_cache" class="syner-historial__aviso">
        Sin conexión: se muestra el historial guardado en este dispositivo.
      </p>
      <p v-else-if="error && items.length" class="syner-historial__aviso">{{ error }}</p>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { IonIcon, onIonViewWillEnter } from "@ionic/vue";
import { arrowForwardOutline, swapHorizontalOutline, timeOutline } from "ionicons/icons";
import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import CompEstadoBadge from "@/components/shared/comp_estado_badge.vue";
import CompPaginacion from "@/components/shared/comp_paginacion.vue";
import {
  obtener_historial_lotes,
  obtener_cache_historial_lotes,
  guardar_cache_historial_lotes,
  type HistorialLoteItem,
} from "@/services/historial_lotes_service";
import { hay_conexion } from "@/services/conectividad_service";
import { useAuthStore } from "@/stores/auth_store";

const auth = useAuthStore();
const LIMITE = 20;
const items = ref<HistorialLoteItem[]>([]);
const pagina = ref(1);
const total_paginas = ref(0);
const cargando = ref(false);
const error = ref("");
const usando_cache = ref(false);
async function cargar(nueva_pagina = pagina.value) {
  cargando.value = true;
  error.value = "";
  usando_cache.value = false;
  const usuario_id = String(auth.usuario?.id ?? "");
  try {
    if (!(await hay_conexion())) {
      const cache = obtener_cache_historial_lotes(usuario_id, nueva_pagina, LIMITE);
      if (!cache) throw new Error("No hay conexión ni historial guardado para esta página.");
      items.value = cache.datos;
      pagina.value = cache.page;
      total_paginas.value = cache.totalPaginas;
      usando_cache.value = true;
      return;
    }

    const resultado = await obtener_historial_lotes(nueva_pagina, LIMITE);
    items.value = resultado.datos;
    pagina.value = resultado.page;
    total_paginas.value = resultado.totalPaginas;
    guardar_cache_historial_lotes(usuario_id, nueva_pagina, LIMITE, resultado);
  } catch (e) {
    const cache = obtener_cache_historial_lotes(usuario_id, nueva_pagina, LIMITE);
    if (cache) {
      items.value = cache.datos;
      pagina.value = cache.page;
      total_paginas.value = cache.totalPaginas;
      usando_cache.value = true;
    } else {
      error.value = e instanceof Error ? e.message : "No se pudo cargar el historial.";
    }
  } finally {
    cargando.value = false;
  }
}
function cambiar_pagina(nueva_pagina: number) { void cargar(nueva_pagina); }
function fecha(valor: string) { const fecha = new Date(valor); return Number.isNaN(fecha.getTime()) ? valor : fecha.toLocaleString("es-AR", { dateStyle: "medium", timeStyle: "short" }); }
onIonViewWillEnter(() => { void cargar(1); });
</script>

<style scoped>
.syner-historial { width: min(100% - 32px, 920px); margin: 0 auto; padding: 24px 0 40px; color: var(--syner-text); }
.syner-historial header { display: flex; align-items: center; gap: 16px; margin-bottom: 22px; }
.syner-historial__icon { display: grid; flex: 0 0 56px; width: 56px; height: 56px; place-items: center; border-radius: 18px; background: color-mix(in srgb, var(--syner-primary) 14%, transparent); color: var(--syner-primary); font-size: 29px; }
.syner-historial header p { margin: 0 0 4px; color: var(--syner-primary); font-size: 12px; font-weight: 800; letter-spacing: .08em; text-transform: uppercase; }
.syner-historial h1 { margin: 0; color: var(--syner-text); font-size: clamp(23px, 5vw, 32px); }
.syner-historial header span:last-child { display: block; margin-top: 5px; color: var(--syner-text-secondary); }
.syner-historial__lista { display: grid; gap: 12px; }
.syner-historial__card { display: flex; gap: 14px; padding: 18px; border: 1px solid var(--syner-border); border-radius: 18px; background: var(--syner-surface); box-shadow: 0 8px 22px color-mix(in srgb, var(--syner-text) 5%, transparent); }
.syner-historial__marca { display: grid; flex: 0 0 40px; width: 40px; height: 40px; place-items: center; border-radius: 13px; background: color-mix(in srgb, var(--syner-primary) 12%, transparent); color: var(--syner-primary); font-size: 20px; }
.syner-historial__contenido { min-width: 0; flex: 1; }
.syner-historial__top { display: flex; justify-content: space-between; flex-wrap: wrap; gap: 6px; color: var(--syner-text-secondary); font-size: 12px; }
.syner-historial h2 { margin: 6px 0 12px; color: var(--syner-text); font-size: 18px; }
.syner-historial__transicion { display: flex; align-items: center; flex-wrap: wrap; gap: 10px; }
.syner-historial__transicion > ion-icon { color: var(--syner-primary); }
.syner-historial__contenido p { margin: 12px 0 0; color: var(--syner-text-secondary); font-size: 13px; }
.syner-historial__contenido strong { color: var(--syner-text); }
.syner-historial__aviso { color: var(--ion-color-danger); }
</style>
