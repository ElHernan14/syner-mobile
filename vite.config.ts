/// <reference types="vitest" />

import legacy from "@vitejs/plugin-legacy";
import vue from "@vitejs/plugin-vue";
import path from "path";
import { defineConfig, loadEnv } from "vite";

export default defineConfig(({ mode }) => {
  const variables = loadEnv(mode, process.cwd(), "VITE_");

  if (mode === "release") {
    const apiUrl = variables.VITE_API_URL ?? process.env.VITE_API_URL;

    if (
      !apiUrl ||
      !URL.canParse(apiUrl) ||
      new URL(apiUrl).protocol !== "https:"
    ) {
      throw new Error(
        "El release requiere VITE_API_URL con una URL HTTPS válida en .env.release.",
      );
    }
  }

  return {
    plugins: [vue(), legacy()],
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },
    test: {
      globals: true,
      environment: "jsdom",
    },
  };
});
