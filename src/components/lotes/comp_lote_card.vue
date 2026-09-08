<template>
  <ion-card class="syner-lote-card">
    <ion-card-header>
      <div class="syner-lote-card__header">
        <div>
          <ion-card-subtitle>
            {{ lote.categoria }}
          </ion-card-subtitle>

          <ion-card-title>
            {{ lote.nombre }}
          </ion-card-title>
        </div>

        <ion-badge :color="color_estado">
          {{ lote.estado }}
        </ion-badge>
      </div>
    </ion-card-header>

    <ion-card-content>
      <p class="syner-lote-card__description">
        {{ lote.descripcion }}
      </p>

      <div class="syner-lote-card__prices">
        <div>
          <span>Precio de cupo</span>
          <strong> ${{ lote.precioCupo.toLocaleString("es-AR") }} </strong>
        </div>

        <div>
          <span>Precio de mercado</span>
          <strong class="syner-lote-card__market-price">
            ${{ lote.precioMercado.toLocaleString("es-AR") }}
          </strong>
        </div>
      </div>

      <div class="syner-lote-card__saving">
        <span>Ahorro</span>
        <strong>{{ lote.porcentajeAhorro }}%</strong>
      </div>

      <div class="syner-lote-card__progress">
        <div class="syner-lote-card__progress-header">
          <span>Cupos ocupados</span>

          <strong> {{ lote.cuposOcupados }}/{{ lote.cantidadCupos }} </strong>
        </div>

        <ion-progress-bar :value="progreso" />
      </div>

      <div class="syner-lote-card__footer">
        <div class="syner-lote-card__dates">
          <span>Finaliza</span>
          <strong>{{ fecha_fin }}</strong>
        </div>

        <div class="syner-lote-card__actions">
          <ion-button
            v-if="puede_editar"
            fill="clear"
            size="small"
            @click="$emit('editar', lote)"
          >
            Editar
          </ion-button>

          <ion-button
            v-if="puede_eliminar"
            fill="clear"
            color="danger"
            size="small"
            @click="$emit('eliminar', lote)"
          >
            Dar de baja
          </ion-button>
        </div>
      </div>
    </ion-card-content>
  </ion-card>
</template>

<script setup lang="ts">
import { computed } from "vue";

import {
  IonBadge,
  IonButton,
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardSubtitle,
  IonCardTitle,
  IonProgressBar,
} from "@ionic/vue";

import type { Lote } from "@/models/LoteModel";

const props = defineProps<{
  lote: Lote;
}>();

defineEmits<{
  editar: [lote: Lote];
  eliminar: [lote: Lote];
}>();

const progreso = computed(() => {
  if (props.lote.cantidadCupos <= 0) {
    return 0;
  }

  return Math.min(props.lote.cuposOcupados / props.lote.cantidadCupos, 1);
});

const fecha_fin = computed(() => {
  return new Date(props.lote.fechaFin).toLocaleDateString("es-AR");
});

const puede_editar = computed(() => {
  return props.lote.estado === "borrador";
});

const puede_eliminar = computed(() => {
  return [
    "borrador",
    "fondeando",
    "completado",
    "comprado",
    "enviado",
  ].includes(props.lote.estado);
});

const color_estado = computed(() => {
  switch (props.lote.estado) {
    case "borrador":
      return "medium";

    case "fondeando":
      return "primary";

    case "completado":
      return "success";

    case "comprado":
      return "tertiary";

    case "enviado":
      return "warning";

    case "entregado":
      return "success";

    case "cancelado":
      return "danger";

    default:
      return "medium";
  }
});
</script>

<style scoped>
.syner-lote-card {
  margin: 0;
  border-radius: 18px;
  overflow: hidden;
}

.syner-lote-card__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.syner-lote-card ion-card-subtitle {
  margin-bottom: 4px;
  color: var(--syner-text-secondary);
  text-transform: capitalize;
}

.syner-lote-card ion-card-title {
  color: var(--syner-text);
  font-size: 20px;
}

.syner-lote-card__description {
  margin: 0;
  color: var(--syner-text-secondary);
  line-height: 1.5;
}

.syner-lote-card__prices {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  margin-top: 20px;
}

.syner-lote-card__prices div {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.syner-lote-card__prices span,
.syner-lote-card__dates span {
  color: var(--syner-text-secondary);
  font-size: 12px;
}

.syner-lote-card__prices strong {
  color: var(--syner-text);
  font-size: 18px;
}

.syner-lote-card__market-price {
  font-size: 14px !important;
  text-decoration: line-through;
  opacity: 0.7;
}

.syner-lote-card__saving {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 16px;
  padding: 10px 12px;
  border-radius: 10px;
  background: color-mix(in srgb, var(--syner-success) 10%, transparent);
  color: var(--syner-success);
  font-size: 13px;
}

.syner-lote-card__saving strong {
  font-size: 15px;
}

.syner-lote-card__progress {
  margin-top: 20px;
}

.syner-lote-card__progress-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 7px;
  color: var(--syner-text-secondary);
  font-size: 13px;
}

.syner-lote-card__progress-header strong {
  color: var(--syner-text);
}

.syner-lote-card ion-progress-bar {
  --background: var(--syner-border);
  --progress-background: var(--syner-primary);
  height: 7px;
  border-radius: 10px;
}

.syner-lote-card__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid var(--syner-border);
}

.syner-lote-card__dates {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.syner-lote-card__dates strong {
  color: var(--syner-text);
  font-size: 13px;
}

.syner-lote-card__actions {
  display: flex;
  align-items: center;
  gap: 2px;
}

@media (max-width: 576px) {
  .syner-lote-card__prices {
    grid-template-columns: 1fr;
    gap: 10px;
  }

  .syner-lote-card__footer {
    align-items: flex-start;
    flex-direction: column;
  }

  .syner-lote-card__actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
