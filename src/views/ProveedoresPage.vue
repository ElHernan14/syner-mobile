<template>
  <comp-page
    :titulo="'Proveedores'"
    :mostrar_refresher="true"
    @actualizar="cargar_proveedores"
  >
    <div class="syner-proveedores">
      <div v-if="cargando">
        <comp-esqueleto :filas="5" />
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
import { onMounted, ref } from "vue";

import { IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import { businessOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { obtener_proveedores, type Proveedor } from "@/data/proveedor";

const proveedores = ref<Proveedor[]>([]);
const cargando = ref(false);

async function cargar_proveedores() {
  cargando.value = true;

  try {
    proveedores.value = await obtener_proveedores();
  } finally {
    cargando.value = false;
  }
}

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
