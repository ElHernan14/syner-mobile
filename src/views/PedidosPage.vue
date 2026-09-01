<template>
  <comp-page
    :titulo="'Pedidos'"
    :mostrar_refresher="true"
    @actualizar="cargar_pedidos"
  >
    <div class="syner-pedidos">
      <div v-if="cargando" class="syner-pedidos__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <div v-else-if="error" class="syner-pedidos__error">
        <p>{{ error }}</p>

        <ion-button :fill="'outline'" @click="cargar_pedidos">
          Reintentar
        </ion-button>
      </div>

      <div v-else-if="!hay_pedidos" class="syner-pedidos__empty">
        <p>No hay pedidos disponibles.</p>
      </div>

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
              <strong>
                {{ pedido.estado }}
              </strong>
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
import { onMounted } from "vue";

import { IonButton, IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import { receiptOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { usePedidos } from "@/composables/usePedidos";

const { pedidos, cargando, error, hay_pedidos, cargar_pedidos } = usePedidos();

onMounted(() => {
  cargar_pedidos();
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
