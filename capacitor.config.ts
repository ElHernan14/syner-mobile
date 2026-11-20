import type { CapacitorConfig } from "@capacitor/cli";
import { loadEnv } from "vite";

const capacitorEnv = loadEnv("release", process.cwd(), "VITE_");
const config: CapacitorConfig = {
  appId: "com.syner.app",
  appName: "SYNER",
  webDir: "dist",
  bundledWebRuntime: false,

  server: {
    androidScheme: "http",
  },
  plugins: {
    GoogleAuth: {
      webClientId:
        process.env.VITE_GOOGLE_WEB_CLIENT_ID ??
        capacitorEnv.VITE_GOOGLE_WEB_CLIENT_ID ??
        "",
    },
    LocalNotifications: {
      smallIcon: "ic_stat_notificacion",
      iconColor: "#0077F8",
    },
  },
  android: {
    webContentsDebuggingEnabled: true,
  },
};

export default config;
