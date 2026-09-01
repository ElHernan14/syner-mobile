<template>
  <comp-page
    :titulo="'Proveedores'"
    :mostrar_refresher="true"
    @actualizar="cargar_proveedores"
  >
    <div class="syner-proveedores">
      <div v-if="cargando" class="syner-proveedores__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <div v-else-if="error" class="syner-proveedores__error">
        <p>{{ error }}</p>

        <ion-button :fill="'outline'" @click="cargar_proveedores">
          Reintentar
        </ion-button>
      </div>

      <div v-else-if="!hay_proveedores" class="syner-proveedores__empty">
        <p>No hay proveedores disponibles.</p>
      </div>

      <ion-list v-else :inset="true">
        <ion-item
          v-for="proveedor in proveedores"
          :key="proveedor.id"
          :lines="'full'"
        >
          <ion-icon slot="start" :icon="businessOutline" />

          <ion-label>
            <h2>
              {{ proveedor.nombre }}
            </h2>

            <p>
              {{ proveedor.descripcion }}
            </p>

            <p
              class="syner-proveedores__estado"
              :class="{
                'syner-proveedores__estado--verificado': proveedor.verificado,
              }"
            >
              {{
                proveedor.verificado
                  ? "Proveedor verificado"
                  : "Proveedor no verificado"
              }}
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

import { businessOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { useProveedores } from "@/composables/useProveedores";

const { proveedores, cargando, error, hay_proveedores, cargar_proveedores } =
  useProveedores();

onMounted(() => {
  cargar_proveedores();
});
</script>

<style scoped>
.syner-proveedores {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-proveedores__loading {
  padding: 0;
}

.syner-proveedores__error,
.syner-proveedores__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 48px 24px;
  text-align: center;
  color: var(--syner-text-secondary);
}

.syner-proveedores ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-proveedores ion-icon {
  color: var(--syner-primary);
}

.syner-proveedores__estado {
  margin-top: 8px;
  font-size: 13px;
  font-weight: 600;
}

.syner-proveedores__estado--verificado {
  color: var(--syner-success);
}
</style>
