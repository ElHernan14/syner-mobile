<template>
  <ion-page>
    <ion-header>
      <ion-toolbar>
        <ion-title>SYNER</ion-title>

        <ion-buttons slot="end">
          <ThemeToggle />
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content class="syner-home">
      <div class="syner-home__container">
        <section class="syner-hero">
          <ion-text>
            <p class="syner-hero__eyebrow">PROYECTO ACADÉMICO · 2026</p>

            <h1 class="syner-hero__title">HERNÁN CONSTANTE</h1>

            <p class="syner-hero__description">
              SYNER es una plataforma de transformación personal basada en
              hábitos, progreso y constancia.
            </p>
          </ion-text>
        </section>

        <section class="syner-section">
          <div class="syner-section__header">
            <div>
              <h2>Investment Pools</h2>
              <p>Invertí en tu propio crecimiento.</p>
            </div>
          </div>

          <ion-grid>
            <ion-row>
              <ion-col
                v-for="pool in pools"
                :key="pool.id"
                size="12"
                size-md="6"
              >
                <ion-card class="syner-pool-card">
                  <ion-card-header>
                    <ion-card-subtitle>
                      {{ pool.category }}
                    </ion-card-subtitle>

                    <ion-card-title>
                      {{ pool.name }}
                    </ion-card-title>
                  </ion-card-header>

                  <ion-card-content>
                    <p class="syner-pool-card__description">
                      {{ pool.description }}
                    </p>

                    <div class="syner-pool-card__stats">
                      <span> Progreso </span>

                      <strong> {{ pool.progress }}% </strong>
                    </div>

                    <div class="syner-pool-card__stats">
                      <span>Balance</span>
                      <strong>{{ pool.balance }}</strong>
                    </div>

                    <div class="syner-pool-card__stats">
                      <span>Rentabilidad</span>
                      <strong>{{ pool.returnRate }}</strong>
                    </div>

                    <ion-progress-bar :value="pool.progress / 100" />
                  </ion-card-content>
                </ion-card>
              </ion-col>
            </ion-row>
          </ion-grid>
        </section>
      </div>
    </ion-content>
  </ion-page>
</template>

<script setup lang="ts">
import {
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardSubtitle,
  IonCardTitle,
  IonCol,
  IonContent,
  IonGrid,
  IonHeader,
  IonPage,
  IonProgressBar,
  IonRow,
  IonText,
  IonTitle,
  IonToolbar,
  IonButtons,
} from "@ionic/vue";

import ThemeToggle from "@/components/theme/ThemeToggle.vue";

interface InvestmentPool {
  id: number;
  name: string;
  category: string;
  description: string;
  progress: number;
  balance: string;
  returnRate: string;
}

const pools: InvestmentPool[] = [
  {
    id: 1,
    name: "Alpha Growth",
    category: "HIGH RETURN",
    description:
      "Fondo de crecimiento compuesto por compañías de innovación y tecnología.",
    progress: 89,
    balance: "$24.580",
    returnRate: "+12.8%",
  },
  {
    id: 2,
    name: "Blue Capital",
    category: "BALANCED",
    description:
      "Pool diversificado entre renta variable y renta fija para un riesgo moderado.",
    progress: 71,
    balance: "$24.580",
    returnRate: "+12.8%",
  },
  {
    id: 3,
    name: "Global Dividend",
    category: "INCOME",
    description:
      "Empresas consolidadas con foco en dividendos sostenibles y estabilidad.",
    progress: 63,
    balance: "$24.580",
    returnRate: "+12.8%",
  },
  {
    id: 4,
    name: "Future Markets",
    category: "EMERGING",
    description:
      "Inversión en mercados emergentes con alto potencial de valorización.",
    progress: 76,
    balance: "$24.580",
    returnRate: "+12.8%",
  },
];
</script>

<style scoped>
.syner-home {
  --background: var(--syner-background);
  color: var(--syner-text);
}

.syner-home__container {
  width: min(100% - 32px, 1000px);
  margin: 0 auto;
  padding: 32px 0 48px;
}

.syner-hero {
  padding: 24px 0 40px;
}

.syner-hero__eyebrow {
  margin: 0 0 8px;
  color: var(--syner-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.syner-hero__title {
  margin: 0;
  color: var(--syner-text);
  font-size: clamp(32px, 7vw, 56px);
  font-weight: 800;
  line-height: 1.05;
}

.syner-hero__description {
  max-width: 620px;
  margin: 20px 0 0;
  color: var(--syner-text-secondary);
  font-size: 16px;
  line-height: 1.6;
}

.syner-section__header h2 {
  margin: 0;
  color: var(--syner-text);
  font-size: 24px;
}

.syner-section__header p {
  margin: 6px 0 20px;
  color: var(--syner-text-secondary);
}

.syner-pool-card {
  height: 100%;
  margin: 0;
  background: var(--syner-surface);
  border: 1px solid var(--syner-border);
  box-shadow: none;
}

.syner-pool-card ion-card-title,
.syner-pool-card ion-card-subtitle {
  color: var(--syner-text);
}

.syner-pool-card ion-card-subtitle {
  color: var(--syner-primary);
}

.syner-pool-card__description {
  margin: 0 0 20px;
  color: var(--syner-text-secondary);
  line-height: 1.5;
}

.syner-pool-card__stats {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  color: var(--syner-text-secondary);
}

.syner-pool-card__stats strong {
  color: var(--syner-text);
}
</style>
