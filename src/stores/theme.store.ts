import { computed, ref } from "vue";
import { defineStore } from "pinia";

const THEME_STORAGE_KEY = "syner-theme";

export type SynerTheme = "light" | "dark";

export const useThemeStore = defineStore("theme", () => {
  const theme = ref<SynerTheme>(
    document.documentElement.classList.contains("syner-theme-dark")
      ? "dark"
      : "light",
  );

  const isDark = computed(() => theme.value === "dark");

  function applyTheme(value: SynerTheme) {
    theme.value = value;

    document.documentElement.classList.toggle(
      "syner-theme-dark",
      value === "dark",
    );

    localStorage.setItem(THEME_STORAGE_KEY, value);
  }

  function toggleTheme() {
    applyTheme(isDark.value ? "light" : "dark");
  }

  function initializeTheme() {
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);

    if (savedTheme === "dark" || savedTheme === "light") {
      applyTheme(savedTheme);
      return;
    }

    applyTheme("light");
  }

  return {
    theme,
    isDark,
    toggleTheme,
    initializeTheme,
  };
});
