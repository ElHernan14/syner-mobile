<template>
  <comp-page titulo="Proveedores" :mostrar_refresher="true" @actualizar="cargar_proveedores">
    <div class="syner-proveedores">
      <div v-if="cargando"><comp-esqueleto :filas="5" /></div>
      <comp-estado-listado v-else-if="error" tipo="error" titulo="No se pudieron cargar los proveedores" :mensaje="error" reintentar @reintentar="cargar_proveedores" />
      <comp-estado-listado v-else-if="!hay_proveedores" tipo="vacio" titulo="No hay proveedores disponibles" mensaje="Los proveedores registrados aparecerán en este listado." />
      <div v-else class="syner-proveedores__grid">
        <article v-for="proveedor in proveedores" :key="proveedor.id" class="syner-proveedores__card" role="button" tabindex="0" @click="ver_ubicacion(proveedor)" @keydown.enter="ver_ubicacion(proveedor)">
          <div class="syner-proveedores__icon"><ion-icon :icon="businessOutline" /></div>
          <div class="syner-proveedores__info">
            <h2>{{ proveedor.nombre }}</h2>
            <p>{{ proveedor.descripcion || "Este proveedor todavía no agregó una descripción." }}</p>
            <div class="syner-proveedores__verification"><ion-icon :icon="proveedor.verificado ? checkmarkCircle : alertCircleOutline" /><span>{{ proveedor.verificado ? "Proveedor verificado" : "Verificación pendiente" }}</span></div>
            <div class="syner-proveedores__location"><ion-icon :icon="locationOutline" /><span>{{ proveedor.latitud != null && proveedor.longitud != null ? "Ver ubicación en el mapa" : "Ubicación no registrada" }}</span><ion-icon class="syner-proveedores__arrow" :icon="chevronForwardOutline" /></div>
          </div>
        </article>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { IonIcon } from "@ionic/vue";
import { alertCircleOutline, businessOutline, checkmarkCircle, chevronForwardOutline, locationOutline } from "ionicons/icons";
import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import { useProveedores } from "@/composables/useProveedores";
const { proveedores, cargando, error, hay_proveedores, cargar_proveedores } = useProveedores();
const router = useRouter();
function ver_ubicacion(proveedor: (typeof proveedores.value)[number]) {
  router.push({ path: "/app/ubicacion", query: { proveedor: "true", nombre: proveedor.nombre, lat: proveedor.latitud == null ? undefined : String(proveedor.latitud), lon: proveedor.longitud == null ? undefined : String(proveedor.longitud) } });
}
onMounted(() => { cargar_proveedores(); });
</script>

<style scoped>
.syner-proveedores { width: min(100% - 32px, 1000px); margin: 0 auto; padding: 24px 0 40px; }
.syner-proveedores__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 380px), 1fr)); gap: 16px; }
.syner-proveedores__card { display: flex; align-items: flex-start; gap: 16px; padding: 20px; border: 1px solid var(--syner-border); border-radius: 20px; background: var(--syner-surface); box-shadow: 0 9px 25px color-mix(in srgb, var(--syner-text) 6%, transparent); cursor: pointer; transition: transform .18s ease, border-color .18s ease; }
.syner-proveedores__card:hover, .syner-proveedores__card:focus-visible { transform: translateY(-2px); border-color: var(--syner-primary); outline: none; }
.syner-proveedores__icon { flex: 0 0 48px; width: 48px; height: 48px; display: grid; place-items: center; border-radius: 16px; background: color-mix(in srgb, var(--syner-primary) 12%, transparent); color: var(--syner-primary); font-size: 24px; }
.syner-proveedores__info { min-width: 0; }
.syner-proveedores h2 { margin: 1px 0 8px; color: var(--syner-text); font-size: 20px; }
.syner-proveedores p { margin: 0; color: var(--syner-text-secondary); line-height: 1.55; }
.syner-proveedores__verification { display: inline-flex; align-items: center; gap: 7px; margin-top: 16px; padding: 8px 11px; border-radius: 999px; background: color-mix(in srgb, var(--syner-success) 10%, transparent); color: var(--syner-success); font-size: 13px; font-weight: 700; }
.syner-proveedores__verification ion-icon { font-size: 20px; }
.syner-proveedores__location { display: flex; align-items: center; gap: 7px; margin-top: 12px; color: var(--syner-primary); font-size: 13px; font-weight: 700; }
.syner-proveedores__location ion-icon { font-size: 18px; }
.syner-proveedores__arrow { margin-left: auto; }
</style>
