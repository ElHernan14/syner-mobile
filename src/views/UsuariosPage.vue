<template>
  <comp-page titulo="Usuarios" :mostrar_refresher="true" @actualizar="cargar_usuarios">
    <div class="syner-usuarios">
      <div v-if="cargando"><comp-esqueleto :filas="5" /></div>
      <div v-if="usando_cache && hay_usuarios" class="syner-usuarios__cache">Sin conexión: se muestran los últimos usuarios cargados por esta cuenta administradora.</div>
      <comp-estado-listado v-if="!cargando && error && !hay_usuarios" tipo="error" titulo="No se pudieron cargar los usuarios" :mensaje="error" reintentar @reintentar="cargar_usuarios" />
      <comp-estado-listado v-else-if="!cargando && !hay_usuarios" tipo="vacio" titulo="No hay usuarios disponibles" mensaje="Los usuarios registrados aparecerán en este listado." />
      <div v-else-if="hay_usuarios" class="syner-usuarios__grid">
        <article v-for="usuario in usuarios" :key="usuario.id" class="syner-usuarios__card">
          <div class="syner-usuarios__avatar">
            <img v-if="avatar_url(usuario.avatar)" :src="avatar_url(usuario.avatar)!" :alt="`Foto de ${usuario.nombre}`" />
            <ion-icon v-else :icon="personCircleOutline" />
          </div>
          <div class="syner-usuarios__info">
            <div class="syner-usuarios__heading"><div><h2>{{ usuario.nombre }}</h2><p>{{ usuario.correo }}</p></div><comp-estado-badge :estado="usuario.estado" /></div>
            <div class="syner-usuarios__facts">
              <span><b>Rol</b>{{ usuario.rol ?? "Sin rol" }}</span>
              <span><b>DNI</b>{{ usuario.dni || "No informado" }}</span>
              <span><b>Teléfono</b>{{ usuario.telefono || "No informado" }}</span>
              <span class="syner-usuarios__address"><b>Dirección</b>{{ usuario.direccion || "Sin dirección registrada" }}</span>
            </div>
          </div>
        </article>
      </div>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { IonIcon } from "@ionic/vue";
import { personCircleOutline } from "ionicons/icons";
import CompPage from "@/components/shared/comp_page.vue";
import CompEsqueleto from "@/components/shared/comp_esqueleto.vue";
import CompEstadoListado from "@/components/shared/comp_estado_listado.vue";
import CompEstadoBadge from "@/components/shared/comp_estado_badge.vue";
import { useUsuarios } from "@/composables/useUsuarios";
import { url_avatar_servidor } from "@/services/perfil_foto_service";

const { usuarios, cargando, error, hay_usuarios, usando_cache, cargar_usuarios } = useUsuarios();
function avatar_url(avatar?: string | null) { return url_avatar_servidor(avatar); }
onMounted(() => { cargar_usuarios(); });
</script>

<style scoped>
.syner-usuarios { width: min(100% - 32px, 1000px); margin: 0 auto; padding: 24px 0 40px; }
.syner-usuarios__grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 420px), 1fr)); gap: 16px; }
.syner-usuarios__card { display: flex; align-items: flex-start; gap: 15px; padding: 18px; border: 1px solid var(--syner-border); border-radius: 18px; background: var(--syner-surface); box-shadow: 0 8px 24px color-mix(in srgb, var(--syner-text) 6%, transparent); color: var(--syner-text); }
.syner-usuarios__avatar { width: 58px; height: 58px; flex: 0 0 58px; display: grid; place-items: center; overflow: hidden; border-radius: 18px; background: color-mix(in srgb, var(--syner-primary) 13%, transparent); color: var(--syner-primary); font-size: 34px; }
.syner-usuarios__avatar img { width: 100%; height: 100%; object-fit: cover; }
.syner-usuarios__info { min-width: 0; flex: 1; }
.syner-usuarios__heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 8px; }
.syner-usuarios__heading h2 { margin: 0 0 4px; color: var(--syner-text); font-size: 18px; }
.syner-usuarios__heading p { margin: 0; color: var(--syner-text-secondary); overflow-wrap: anywhere; }
.syner-usuarios__facts { display: grid; grid-template-columns: 1fr 1fr; gap: 10px 14px; margin-top: 14px; }
.syner-usuarios__facts span { display: grid; gap: 3px; color: var(--syner-text); font-size: 13px; overflow-wrap: anywhere; }
.syner-usuarios__facts b { color: var(--syner-text-secondary); font-size: 11px; text-transform: uppercase; letter-spacing: .04em; }
.syner-usuarios__cache { margin-bottom: 14px; padding: 10px 12px; border-radius: 12px; background: color-mix(in srgb, var(--ion-color-warning) 13%, transparent); color: var(--syner-text); font-size: 13px; }
@media (max-width: 600px) { .syner-usuarios__card { padding: 14px; } .syner-usuarios__heading { flex-direction: column; } .syner-usuarios__facts { grid-template-columns: 1fr; } }
</style>
