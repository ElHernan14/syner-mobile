import { Network } from "@capacitor/network";
import { ref } from "vue";

export const conectado = ref(
  typeof navigator === "undefined" || navigator.onLine,
);

export async function hay_conexion(): Promise<boolean> {
  try {
    return (await Network.getStatus()).connected;
  } catch {
    return navigator.onLine;
  }
}

export function escuchar_cambios_conexion(
  callback: (conectado: boolean) => void,
): Promise<void> {
  return Network.getStatus()
    .then((status) => {
      conectado.value = status.connected;
    })
    .catch(() => {
      conectado.value = navigator.onLine;
    })
    .then(() =>
      Network.addListener("networkStatusChange", (status) => {
        conectado.value = status.connected;
        callback(status.connected);
      }),
    )
    .then(() => undefined);
}
