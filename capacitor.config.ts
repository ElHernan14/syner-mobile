import type { CapacitorConfig } from "@capacitor/cli";

const config: CapacitorConfig = {
  appId: "com.example.app",
  appName: "SYNER-BASE",
  webDir: "dist",
  bundledWebRuntime: false,

  server: {
    androidScheme: "http",
  },
};

export default config;
