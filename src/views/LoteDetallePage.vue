<template>
  <comp-page
    titulo="Detalle del lote"
    :mostrar_refresher="true"
    @actualizar="cargar_lote"
  >
    <div class="syner-lote-detalle">
      <div v-if="cargando" class="syner-lote-detalle__loading">
        <comp-esqueleto :filas="7" />
      </div>

      <div v-else-if="error" class="syner-lote-detalle__error">
        <p>{{ error }}</p>

        <ion-button fill="outline" @click="cargar_lote">
          Reintentar
        </ion-button>
      </div>

      <template v-else-if="lote">
        <div class="syner-lote-detalle__header">
          <div>
            <span class="syner-lote-detalle__categoria">
              {{ lote.categoria }}
            </span>

            <h1>
              {{ lote.nombre }}
            </h1>
          </div>

          <span class="syner-lote-detalle__estado">
            {{ lote.estado }}
          </span>
        </div>

        <div class="syner-lote-detalle__card">
          <h2>Información</h2>

          <p class="syner-lote-detalle__descripcion">
            {{ lote.descripcion }}
          </p>

          <div class="syner-lote-detalle__grid">
            <div>
              <span>Precio de mercado</span>

              <strong> ${{ lote.precioMercado }} </strong>
            </div>

            <div>
              <span>Precio del cupo</span>

              <strong> ${{ lote.precioCupo }} </strong>
            </div>

            <div>
              <span>Ahorro</span>

              <strong> {{ lote.porcentajeAhorro }}% </strong>
            </div>

            <div>
              <span>Cupos</span>

              <strong>
                {{ lote.cuposOcupados }} /
                {{ lote.cantidadCupos }}
              </strong>
            </div>
          </div>
        </div>

        <div class="syner-lote-detalle__card">
          <h2>Proveedor</h2>

          <p>
            {{ lote.proveedor.nombre }}
          </p>
        </div>

        <div class="syner-lote-detalle__card">
          <h2>Vigencia</h2>

          <div class="syner-lote-detalle__fechas">
            <div>
              <span>Inicio</span>

              <strong>
                {{ formatear_fecha(lote.fechaInicio) }}
              </strong>
            </div>

            <div>
              <span>Fin</span>

              <strong>
                {{ formatear_fecha(lote.fechaFin) }}
              </strong>
            </div>
          </div>
        </div>
      </template>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";

import { IonButton } from "@ionic/vue";

import { useRoute } from "vue-router";

import CompPage from "@/components/shared/comp_page.vue";

import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { obtener_lote } from "@/services/lotes_service";

const route = useRoute();

const cargando = ref(false);

const error = ref<string | null>(null);

const lote = ref<Awaited<ReturnType<typeof obtener_lote>> | null>(null);

const lote_id = computed(() => {
  const id = Number(route.params.id);

  return Number.isInteger(id) && id > 0 ? id : null;
});

function formatear_fecha(fecha: string): string {
  const valor = new Date(fecha);

  if (Number.isNaN(valor.getTime())) {
    return fecha;
  }

  return valor.toLocaleDateString("es-AR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

async function cargar_lote() {
  if (lote_id.value === null) {
    error.value = "El identificador del lote no es válido.";

    return;
  }

  cargando.value = true;
  error.value = null;

  try {
    lote.value = await obtener_lote(lote_id.value);
  } catch (err) {
    error.value =
      err instanceof Error ? err.message : "No se pudo cargar el lote.";
  } finally {
    cargando.value = false;
  }
}

onMounted(() => {
  cargar_lote();
});
</script>

<style scoped>
.syner-lote-detalle {
  width: min(100% - 32px, 900px);
  margin: 0 auto;
  padding: 24px 0 48px;
}

.syner-lote-detalle__loading {
  padding: 8px 0;
}

.syner-lote-detalle__error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 48px 24px;
  color: var(--syner-text-secondary);
  text-align: center;
}

.syner-lote-detalle__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 20px;
}

.syner-lote-detalle__categoria {
  color: var(--syner-primary);
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
}

.syner-lote-detalle__header h1 {
  margin: 6px 0 0;
  color: var(--syner-text);
  font-size: 26px;
}

.syner-lote-detalle__estado {
  flex-shrink: 0;
  padding: 6px 10px;
  border-radius: 999px;
  background: var(--syner-background);
  color: var(--syner-text-secondary);
  font-size: 12px;
  font-weight: 600;
  text-transform: capitalize;
}

.syner-lote-detalle__card {
  margin-top: 16px;
  padding: 20px;
  border: 1px solid var(--syner-border);
  border-radius: 14px;
  background: var(--syner-surface);
}

.syner-lote-detalle__card h2 {
  margin: 0 0 14px;
  color: var(--syner-text);
  font-size: 16px;
}

.syner-lote-detalle__card p {
  margin: 0;
  color: var(--syner-text-secondary);
}

.syner-lote-detalle__descripcion {
  line-height: 1.6;
}

.syner-lote-detalle__grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin-top: 20px;
}

.syner-lote-detalle__grid div,
.syner-lote-detalle__fechas div {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.syner-lote-detalle__grid span,
.syner-lote-detalle__fechas span {
  color: var(--syner-text-secondary);
  font-size: 12px;
}

.syner-lote-detalle__grid strong,
.syner-lote-detalle__fechas strong {
  color: var(--syner-text);
  font-size: 15px;
}

.syner-lote-detalle__fechas {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

@media (max-width: 600px) {
  .syner-lote-detalle__header {
    flex-direction: column;
  }

  .syner-lote-detalle__grid,
  .syner-lote-detalle__fechas {
    grid-template-columns: 1fr;
  }
}
</style>
