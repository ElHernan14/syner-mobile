import type { CapacitorConfig } from "@capacitor/cli";

const config: CapacitorConfig = {
  appId: "com.example.app",
  appName: "SYNER",
  webDir: "dist",
  bundledWebRuntime: false,

  server: {
    androidScheme: "http",
  },
  plugins: {
    LocalNotifications: {
      smallIcon: "ic_stat_notificacion",
      iconColor: "#0077F8",
    },
  },
};

export default config;
