<template>
  <comp-page
    :titulo="'Pedidos'"
    :mostrar_refresher="true"
    @actualizar="cargar_pedidos"
  >
    <div class="syner-pedidos">
      <div v-if="cargando">
        <comp-esqueleto :filas="5" />
      </div>

      <ion-list v-else :inset="true">
        <ion-item v-for="pedido in pedidos" :key="pedido.id" :lines="'full'">
          <ion-icon slot="start" :icon="receiptOutline" />

          <ion-label>
            <h2>
              {{ pedido.id }}
            </h2>

            <p>Lote: {{ pedido.loteId }}</p>

            <p>
              Cliente:
              {{ pedido.usuarioId ?? "Sin cliente asociado" }}
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
import { onMounted, ref } from "vue";

import { IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import { receiptOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { obtener_pedidos, type Pedido } from "@/data/pedido";

const pedidos = ref<Pedido[]>([]);
const cargando = ref(false);

async function cargar_pedidos() {
  cargando.value = true;

  try {
    pedidos.value = await obtener_pedidos();
  } finally {
    cargando.value = false;
  }
}

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
