<template>
  <comp-page
    :titulo="'Pedidos'"
    :mostrar_menu="!pedido_id"
    :mostrar_volver="Boolean(pedido_id)"
    ruta_volver="/app/pedidos"
    :mostrar_refresher="true"
    @actualizar="cargar_pedidos"
  >
    <div class="syner-pedidos">
      <h1 v-if="pedido_id" class="syner-pedidos__target">Pedido #{{ pedido_id }}</h1>
      <div v-if="cargando" class="syner-pedidos__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <comp-estado-listado
        v-else-if="error && !hay_pedidos"
        tipo="error"
        titulo="No se pudieron cargar los pedidos"
        :mensaje="error"
        :reintentar="true"
        @reintentar="cargar_pedidos"
      />

      <comp-estado-listado
        v-else-if="!hay_pedidos"
        tipo="vacio"
        :titulo="pedido_id ? 'No se encontró el pedido' : 'No hay pedidos todavía'"
        :mensaje="pedido_id ? 'El pedido puede haber sido eliminado o ya no estar disponible.' : 'Cuando se registren pedidos, aparecerán en este listado.'"
      />

      <div v-else class="syner-pedidos__grid">
        <article v-for="pedido in pedidos" :key="pedido.id" class="syner-pedidos__card">
          <header class="syner-pedidos__card-header">
            <div class="syner-pedidos__icon"><ion-icon :icon="receiptOutline" /></div>
            <div><span>Pedido #{{ pedido.id }}</span><h2>{{ pedido.lote.nombre }}</h2></div>
            <comp-estado-badge :estado="pedido.estado" />
          </header>
          <p class="syner-pedidos__categoria">{{ pedido.lote.categoria }}<template v-if="pedido.lote.proveedor"> · {{ pedido.lote.proveedor }}</template></p>
          <div class="syner-pedidos__precios">
            <div><span>Tu precio</span><strong>{{ moneda(pedido.lote.precioCupo) }}</strong></div>
            <div><span>Precio de mercado</span><strong class="syner-pedidos__mercado">{{ moneda(pedido.lote.precioMercado) }}</strong></div>
          </div>
          <div class="syner-pedidos__ahorro">Ahorrás <strong>{{ pedido.lote.porcentajeAhorro }}%</strong></div>
          <div class="syner-pedidos__metadata">
            <span v-if="es_admin">Cliente: {{ pedido.usuario.nombre }}</span>
            <span v-if="pedido.numeroSeguimiento">Seguimiento: {{ pedido.numeroSeguimiento }}</span>
            <span v-if="pedido.codigoEntrega">Código: {{ pedido.codigoEntrega }}</span>
          </div>
        </article>
      </div>
      <div v-if="usando_cache && hay_pedidos" class="syner-pedidos__cache">
        Sin conexión: se muestran los últimos pedidos guardados en este dispositivo.
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useRoute } from "vue-router";

import { IonIcon } from "@ionic/vue";

import { receiptOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import CompEstadoBadge from "@/components/shared/comp_estado_badge.vue";

import { usePedidos } from "@/composables/usePedidos";
import { useAuthStore } from "@/stores/auth_store";

const route = useRoute();
const { pedidos, cargando, error, hay_pedidos, usando_cache, cargar_pedidos } = usePedidos();
const auth = useAuthStore();
const es_admin = computed(() => auth.rol === "admin");
function moneda(valor: number): string { return new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 0 }).format(valor); }
const pedido_id = computed(() => {
  const id = Number(route.query.pedidoId);
  return Number.isInteger(id) && id > 0 ? id : null;
});

onMounted(() => {
  cargar_pedidos(pedido_id.value ?? undefined);
});

watch(() => route.query.pedidoId, () => {
  cargar_pedidos(pedido_id.value ?? undefined);
});
</script>

<style scoped>
.syner-pedidos {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-pedidos__loading {
  padding: 0;
}

.syner-pedidos__target {
  margin: 0 0 16px;
  color: var(--syner-text);
  font-size: 22px;
}

.syner-pedidos__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 360px), 1fr)); gap: 16px; }
.syner-pedidos__card { min-width: 0; padding: 20px; border: 1px solid var(--syner-border); border-radius: 20px; background: var(--syner-surface); box-shadow: 0 10px 28px color-mix(in srgb, var(--syner-text) 7%, transparent); color: var(--syner-text); }
.syner-pedidos__card-header { display: flex; align-items: center; gap: 12px; }
.syner-pedidos__card-header > div:nth-child(2) { min-width: 0; flex: 1; }
.syner-pedidos__card-header span, .syner-pedidos__categoria, .syner-pedidos__precios span, .syner-pedidos__metadata { color: var(--syner-text-secondary); font-size: 13px; }
.syner-pedidos__card h2 { margin: 3px 0 0; color: var(--syner-text); font-size: 18px; }
.syner-pedidos__icon { width: 42px; height: 42px; flex: 0 0 42px; display: grid; place-items: center; border-radius: 14px; background: color-mix(in srgb, var(--syner-primary) 12%, transparent); color: var(--syner-primary); font-size: 21px; }
.syner-pedidos__categoria { margin: 12px 0 16px; text-transform: capitalize; }
.syner-pedidos__precios { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.syner-pedidos__precios div { display: grid; gap: 4px; }
.syner-pedidos__precios strong { color: var(--syner-text); font-size: 17px; }
.syner-pedidos__precios .syner-pedidos__mercado { text-decoration: line-through; opacity: .72; font-size: 14px; }
.syner-pedidos__ahorro { display: flex; justify-content: space-between; margin-top: 14px; padding: 11px 13px; border-radius: 12px; background: color-mix(in srgb, var(--syner-success) 10%, transparent); color: var(--syner-success); }
.syner-pedidos__metadata { display: grid; gap: 6px; margin-top: 14px; }
.syner-pedidos__cache { margin-bottom: 14px; padding: 10px 12px; border-radius: 12px; background: color-mix(in srgb, var(--ion-color-warning) 13%, transparent); color: var(--syner-text); font-size: 13px; }

.syner-pedidos__error,
.syner-pedidos__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 48px 24px;
  text-align: center;
  color: var(--syner-text-secondary);
}

</style>
