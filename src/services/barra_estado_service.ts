import { Capacitor } from "@capacitor/core";
import { StatusBar, Style } from "@capacitor/status-bar";

export async function configurar_barra_estado(): Promise<void> {
  if (!Capacitor.isNativePlatform()) {
    return;
  }

  await StatusBar.setOverlaysWebView({ overlay: false });
  await StatusBar.setBackgroundColor({ color: "#f5f7fb" });
  await StatusBar.setStyle({ style: Style.Dark });
}
