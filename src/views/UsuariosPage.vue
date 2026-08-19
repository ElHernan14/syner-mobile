<template>
  <comp-page
    :titulo="'Usuarios'"
    :mostrar_refresher="true"
    @actualizar="cargar_usuarios"
  >
    <div class="syner-usuarios">
      <div v-if="cargando">
        <comp-esqueleto :filas="5" />
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
import { onMounted, ref } from "vue";

import { IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import { personCircleOutline } from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";

import { obtener_usuarios, type Usuario } from "@/data/usuario";

const usuarios = ref<Usuario[]>([]);
const cargando = ref(false);

async function cargar_usuarios() {
  cargando.value = true;

  try {
    usuarios.value = await obtener_usuarios();
  } finally {
    cargando.value = false;
  }
}

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
