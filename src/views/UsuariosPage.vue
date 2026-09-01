<template>
  <comp-page
    :titulo="'Usuarios'"
    :mostrar_refresher="true"
    @actualizar="cargar_usuarios"
  >
    <div class="syner-usuarios">
      <div v-if="cargando" class="syner-usuarios__loading">
        <comp-esqueleto :filas="5" />
      </div>

      <div v-else-if="error" class="syner-usuarios__error">
        <p>{{ error }}</p>

        <ion-button :fill="'outline'" @click="cargar_usuarios">
          Reintentar
        </ion-button>
      </div>

      <div v-else-if="!hay_usuarios" class="syner-usuarios__empty">
        <p>No hay usuarios disponibles.</p>
      </div>

      <ion-list v-else :inset="true">
        <ion-item v-for="usuario in usuarios" :key="usuario.id" :lines="'full'">
          <ion-icon slot="start" :icon="personCircleOutline" />

          <ion-label>
            <h2>
              {{ usuario.nombre }}
            </h2>

            <p>
              {{ usuario.correo }}
            </p>

            <p>DNI: {{ usuario.dni }}</p>

            <p>Rol: {{ usuario.rol }}</p>

            <p
              class="syner-usuarios__estado"
              :class="{
                'syner-usuarios__estado--verificado':
                  usuario.estado === 'verificado',

                'syner-usuarios__estado--bloqueado':
                  usuario.estado === 'bloqueado',
              }"
            >
              Estado: {{ usuario.estado }}
            </p>

            <p>
              Dirección:
              {{ usuario.direccion ?? "Sin dirección registrada" }}
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

import { personCircleOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { useUsuarios } from "@/composables/useUsuarios";

const { usuarios, cargando, error, hay_usuarios, cargar_usuarios } =
  useUsuarios();

onMounted(() => {
  cargar_usuarios();
});
</script>

<style scoped>
.syner-usuarios {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-usuarios__loading {
  padding: 0;
}

.syner-usuarios__error,
.syner-usuarios__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 48px 24px;
  text-align: center;
  color: var(--syner-text-secondary);
}

.syner-usuarios ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-usuarios ion-icon {
  color: var(--syner-primary);
}

.syner-usuarios__estado {
  margin-top: 8px;
  font-size: 13px;
  font-weight: 600;
}

.syner-usuarios__estado--verificado {
  color: var(--syner-success);
}

.syner-usuarios__estado--bloqueado {
  color: #ef4444;
}
</style>
