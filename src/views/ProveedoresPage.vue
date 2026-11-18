<template>
  <comp-page titulo="Proveedores" :mostrar_refresher="true" @actualizar="cargar_proveedores">
    <div class="syner-proveedores">
      <div v-if="cargando"><comp-esqueleto :filas="5" /></div>
      <comp-estado-listado v-else-if="error" tipo="error" titulo="No se pudieron cargar los proveedores" :mensaje="error" reintentar @reintentar="cargar_proveedores" />
      <comp-estado-listado v-else-if="!hay_proveedores" tipo="vacio" titulo="No hay proveedores disponibles" mensaje="Los proveedores registrados aparecerán en este listado." />
      <div v-else class="syner-proveedores__grid">
        <article v-for="proveedor in proveedores" :key="proveedor.id" class="syner-proveedores__card">
          <div class="syner-proveedores__icon"><ion-icon :icon="businessOutline" /></div>
          <div class="syner-proveedores__info">
            <h2>{{ proveedor.nombre }}</h2>
            <p>{{ proveedor.descripcion || "Este proveedor todavía no agregó una descripción." }}</p>
            <div class="syner-proveedores__verification"><ion-icon :icon="proveedor.verificado ? checkmarkCircle : alertCircleOutline" /><span>{{ proveedor.verificado ? "Proveedor verificado" : "Verificación pendiente" }}</span></div>
          </div>
        </article>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { IonIcon } from "@ionic/vue";
import { alertCircleOutline, businessOutline, checkmarkCircle } from "ionicons/icons";
import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import { useProveedores } from "@/composables/useProveedores";
const { proveedores, cargando, error, hay_proveedores, cargar_proveedores } = useProveedores();
onMounted(() => { cargar_proveedores(); });
</script>

<style scoped>
.syner-proveedores { width: min(100% - 32px, 1000px); margin: 0 auto; padding: 24px 0 40px; }
.syner-proveedores__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 380px), 1fr)); gap: 16px; }
.syner-proveedores__card { display: flex; align-items: flex-start; gap: 16px; padding: 20px; border: 1px solid var(--syner-border); border-radius: 20px; background: var(--syner-surface); box-shadow: 0 9px 25px color-mix(in srgb, var(--syner-text) 6%, transparent); }
.syner-proveedores__icon { flex: 0 0 48px; width: 48px; height: 48px; display: grid; place-items: center; border-radius: 16px; background: color-mix(in srgb, var(--syner-primary) 12%, transparent); color: var(--syner-primary); font-size: 24px; }
.syner-proveedores__info { min-width: 0; }
.syner-proveedores h2 { margin: 1px 0 8px; color: var(--syner-text); font-size: 20px; }
.syner-proveedores p { margin: 0; color: var(--syner-text-secondary); line-height: 1.55; }
.syner-proveedores__verification { display: inline-flex; align-items: center; gap: 7px; margin-top: 16px; padding: 8px 11px; border-radius: 999px; background: color-mix(in srgb, var(--syner-success) 10%, transparent); color: var(--syner-success); font-size: 13px; font-weight: 700; }
.syner-proveedores__verification ion-icon { font-size: 20px; }
</style>
