import { ref } from "vue";

import { defineStore } from "pinia";

const VIBRACION_KEY = "syner_vibracion_activa";

function obtener_vibracion_persistida(): boolean {
  const valor = localStorage.getItem(VIBRACION_KEY);

  if (valor === null) {
    return true;
  }

  return valor === "true";
}

export const usePreferenciasStore = defineStore("preferencias", () => {
  const vibracion_activa = ref(obtener_vibracion_persistida());

  function establecer_vibracion_activa(activa: boolean) {
    vibracion_activa.value = activa;

    localStorage.setItem(VIBRACION_KEY, String(activa));
  }

  return {
    vibracion_activa,
    establecer_vibracion_activa,
  };
});
