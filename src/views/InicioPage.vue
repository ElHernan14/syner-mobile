<template>
  <comp-page :titulo="'Inicio'">
    <div class="syner-inicio">
      <section class="syner-inicio__hero">
        <p class="syner-inicio__eyebrow">PROYECTO ACADÉMICO · 2026</p>

        <h1>HERNÁN CONSTANTE</h1>

        <p class="syner-inicio__description">
          SYNER es una plataforma de compras colectivas que permite acceder a
          productos y servicios mediante lotes y cupos compartidos.
        </p>
      </section>

      <section class="syner-inicio__section">
        <h2>SYNER</h2>

        <p>
          Organizá tus compras, accedé a mejores precios y seguí el estado de
          tus pedidos desde un solo lugar.
        </p>
      </section>

      <ion-list class="syner-inicio__list" :inset="true">
        <ion-item>
          <ion-icon slot="start" :icon="layersOutline" />

          <ion-label>
            <h3>Lotes</h3>
            <p>Explorá oportunidades y cupos disponibles.</p>
          </ion-label>
        </ion-item>

        <ion-item>
          <ion-icon slot="start" :icon="receiptOutline" />

          <ion-label>
            <h3>Pedidos</h3>
            <p>Consultá el estado y seguimiento de tus compras.</p>
          </ion-label>
        </ion-item>

        <ion-item>
          <ion-icon slot="start" :icon="businessOutline" />

          <ion-label>
            <h3>Proveedores</h3>
            <p>Conocé los proveedores disponibles en SYNER.</p>
          </ion-label>
        </ion-item>

        <ion-item>
          <ion-icon slot="start" :icon="peopleOutline" />

          <ion-label>
            <h3>Usuarios</h3>
            <p>Administrá los usuarios registrados.</p>
          </ion-label>
        </ion-item>
      </ion-list>
    </div>
  </comp-page>
</template>

<script setup lang="ts">
import { IonIcon, IonItem, IonLabel, IonList } from "@ionic/vue";

import {
  businessOutline,
  layersOutline,
  peopleOutline,
  receiptOutline,
} from "ionicons/icons";

import CompPage from "@/components/shared/comp_page.vue";
</script>

<style scoped>
.syner-inicio {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 24px 0 40px;
}

.syner-inicio__hero {
  padding: 24px 0 32px;
}

.syner-inicio__eyebrow {
  margin: 0 0 8px;
  color: var(--syner-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.syner-inicio__hero h1 {
  margin: 0;
  color: var(--syner-text);
  font-size: clamp(32px, 8vw, 56px);
  font-weight: 800;
  line-height: 1.05;
}

.syner-inicio__description {
  max-width: 650px;
  margin: 20px 0 0;
  color: var(--syner-text-secondary);
  font-size: 16px;
  line-height: 1.6;
}

.syner-inicio__section {
  margin-bottom: 20px;
}

.syner-inicio__section h2 {
  margin: 0 0 8px;
  color: var(--syner-text);
  font-size: 24px;
}

.syner-inicio__section p {
  margin: 0;
  color: var(--syner-text-secondary);
  line-height: 1.5;
}

.syner-inicio__list {
  margin: 0;
}

.syner-inicio__list ion-item {
  --background: var(--syner-surface);
  --border-color: var(--syner-border);
}

.syner-inicio__list ion-icon {
  color: var(--syner-primary);
}
</style>
