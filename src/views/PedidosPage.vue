<template>
  <comp-page
    :titulo="'Pedidos'"
    :mostrar_refresher="true"
    @actualizar="cargar_pedidos"
  >
    <div class="syner-pedidos">
      <h1 v-if="pedido_id" class="syner-pedidos__target">Pedido #{{ pedido_id }}</h1>
      <div v-if="cargando" class="syner-pedidos__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <comp-estado-listado
        v-else-if="error"
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

      <ion-list v-else :inset="true">
        <ion-item v-for="pedido in pedidos" :key="pedido.id" :lines="'full'">
          <ion-icon slot="start" :icon="receiptOutline" />

          <ion-label>
            <h2>Pedido #{{ pedido.id }}</h2>

            <p>
              Lote:
              {{ pedido.lote.nombre }}
            </p>

            <p>
              Cliente:
              {{ pedido.usuario.nombre }}
            </p>

            <p>
              Estado:
              <comp-estado-badge :estado="pedido.estado" />
            </p>

            <p v-if="pedido.numeroSeguimiento">
              Seguimiento:
              {{ pedido.numeroSeguimiento }}
            </p>

            <p v-if="pedido.codigoEntrega">
              Código de entrega:
              {{ pedido.codigoEntrega }}
            </p>
          </ion-label>
        </ion-item>
      </ion-list>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useRoute } from "vue-router";

import { IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import { receiptOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import CompEstadoBadge from "@/components/shared/comp_estado_badge.vue";

import { usePedidos } from "@/composables/usePedidos";

const route = useRoute();
const { pedidos, cargando, error, hay_pedidos, cargar_pedidos } = usePedidos();
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

.syner-pedidos ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-pedidos ion-icon {
  color: var(--syner-primary);
}

.syner-pedidos p {
  margin-top: 6px;
}

.syner-pedidos strong {
  color: var(--syner-text);
  text-transform: capitalize;
}
</style>
