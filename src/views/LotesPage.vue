<template>
  <comp-page
    :titulo="'Lotes'"
    :mostrar_refresher="true"
    @actualizar="cargar_lotes"
  >
    <div class="syner-lotes">
      <div v-if="cargando" class="syner-lotes__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <ion-list v-else :inset="true">
        <ion-item v-for="lote in lotes" :key="lote.id" :lines="'full'">
          <ion-label>
            <h2>{{ lote.nombre }}</h2>

            <p>
              {{ lote.categoria }}
            </p>

            <p>
              {{ lote.descripcion }}
            </p>

            <div class="syner-lotes__prices">
              <strong> ${{ lote.precioCupo.toLocaleString("es-AR") }} </strong>

              <span> Precio de cupo </span>
            </div>

            <div class="syner-lotes__saving">
              Ahorro: {{ lote.porcentajeAhorro }}%
            </div>

            <div class="syner-lotes__progress">
              <div class="syner-lotes__progress-header">
                <span> Cupos </span>

                <strong>
                  {{ lote.cuposOcupados }}/{{ lote.cantidadCupos }}
                </strong>
              </div>

              <ion-progress-bar
                :value="lote.cuposOcupados / lote.cantidadCupos"
              />
            </div>

            <div class="syner-lotes__status">
              Estado:
              <strong>
                {{ lote.estado }}
              </strong>
            </div>
          </ion-label>
        </ion-item>
      </ion-list>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";

import { IonItem, IonLabel, IonList, IonProgressBar } from "@ionic/vue";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { obtener_lotes, type Lote } from "@/data/lote";

const lotes = ref<Lote[]>([]);
const cargando = ref(false);

async function cargar_lotes() {
  cargando.value = true;

  try {
    lotes.value = await obtener_lotes();
  } finally {
    cargando.value = false;
  }
}

onMounted(() => {
  cargar_lotes();
});
</script>

<style scoped>
.syner-lotes {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-lotes__loading {
  padding: 0;
}

.syner-lotes__prices {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-top: 12px;
}

.syner-lotes__prices strong {
  color: var(--syner-text);
  font-size: 18px;
}

.syner-lotes__prices span {
  color: var(--syner-text-secondary);
  font-size: 12px;
}

.syner-lotes__saving {
  margin-top: 6px;
  color: var(--syner-success);
  font-size: 13px;
  font-weight: 600;
}

.syner-lotes__progress {
  margin-top: 14px;
}

.syner-lotes__progress-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 6px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-lotes__progress-header strong {
  color: var(--syner-text);
}

.syner-lotes__status {
  margin-top: 12px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-lotes__status strong {
  color: var(--syner-text);
}
</style>
