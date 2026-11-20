<template>
  <div class="syner-firma">
    <div class="syner-firma__cabecera">
      <div><h3>Firma de conformidad</h3><p>Firmá dentro del recuadro con el dedo o el mouse.</p></div>
      <ion-button fill="clear" size="small" @click="limpiar"><ion-icon slot="start" :icon="refreshOutline" />Limpiar</ion-button>
    </div>
    <canvas ref="canvas" class="syner-firma__canvas" aria-label="Área para firmar" @pointerdown="iniciar_trazo" @pointermove="continuar_trazo" @pointerup="terminar_trazo" @pointercancel="terminar_trazo" />
    <p v-if="error" class="syner-firma__error" role="alert">{{ error }}</p>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { IonButton, IonIcon } from "@ionic/vue";
import { refreshOutline } from "ionicons/icons";

const canvas = ref<HTMLCanvasElement | null>(null);
const error = ref("");
let contexto: CanvasRenderingContext2D | null = null;
let dibujando = false;
let firmada = false;
let observer: ResizeObserver | null = null;

function ajustar_tamano() {
  const elemento = canvas.value;
  if (!elemento) return;
  const ratio = Math.max(1, window.devicePixelRatio || 1);
  const ancho = elemento.clientWidth;
  const alto = elemento.clientHeight;
  elemento.width = Math.round(ancho * ratio);
  elemento.height = Math.round(alto * ratio);
  contexto = elemento.getContext("2d");
  if (contexto) {
    contexto.setTransform(ratio, 0, 0, ratio, 0, 0);
    contexto.lineWidth = 2.5;
    contexto.lineCap = "round";
    contexto.lineJoin = "round";
    contexto.strokeStyle = getComputedStyle(document.documentElement).getPropertyValue("--syner-text").trim() || "#18212f";
  }
  firmada = false;
}
function punto(evento: PointerEvent) {
  const rect = canvas.value!.getBoundingClientRect();
  return { x: evento.clientX - rect.left, y: evento.clientY - rect.top };
}
function iniciar_trazo(evento: PointerEvent) {
  if (!canvas.value || !contexto) return;
  canvas.value.setPointerCapture(evento.pointerId);
  dibujando = true;
  error.value = "";
  const p = punto(evento);
  contexto.beginPath();
  contexto.moveTo(p.x, p.y);
  contexto.lineTo(p.x + 0.1, p.y + 0.1);
  contexto.stroke();
  firmada = true;
}
function continuar_trazo(evento: PointerEvent) {
  if (!dibujando || !contexto) return;
  const p = punto(evento);
  contexto.lineTo(p.x, p.y);
  contexto.stroke();
}
function terminar_trazo() { dibujando = false; contexto?.closePath(); }
function limpiar() {
  if (!canvas.value || !contexto) return;
  contexto.clearRect(0, 0, canvas.value.clientWidth, canvas.value.clientHeight);
  firmada = false;
  error.value = "";
}
function obtener_png(): Promise<Blob> {
  return new Promise((resolve, reject) => {
    if (!firmada || !canvas.value) { error.value = "La firma es obligatoria para confirmar el cupo."; reject(new Error(error.value)); return; }
    canvas.value.toBlob((blob) => blob ? resolve(blob) : reject(new Error("No se pudo preparar la firma.")), "image/png");
  });
}
defineExpose({ obtener_png, limpiar });
onMounted(() => {
  ajustar_tamano();
  if (canvas.value) { observer = new ResizeObserver(ajustar_tamano); observer.observe(canvas.value); }
});
onBeforeUnmount(() => observer?.disconnect());
</script>

<style scoped>
.syner-firma { width: 100%; }
.syner-firma__cabecera { display: flex; align-items: center; justify-content: space-between; gap: 8px; margin-bottom: 10px; }
.syner-firma h3 { margin: 0; color: var(--syner-text); font-size: 17px; }
.syner-firma p { margin: 4px 0 0; color: var(--syner-text-secondary); font-size: 13px; }
.syner-firma__canvas { display: block; width: 100%; height: 180px; border: 1px dashed var(--syner-primary); border-radius: 14px; background: var(--syner-background); touch-action: none; }
.syner-firma .syner-firma__error { margin-top: 8px; color: var(--ion-color-danger); }
@media (max-width: 480px) { .syner-firma__canvas { height: 150px; } }
</style>
