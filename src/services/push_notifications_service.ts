import {
  PushNotifications,
  type ActionPerformed,
  type PushNotificationSchema,
  type Token,
} from "@capacitor/push-notifications";

import { registrar_dispositivo } from "@/services/dispositivos_service";

let listeners_registrados = false;

export async function solicitar_permisos_push(): Promise<boolean> {
  const permisos = await PushNotifications.checkPermissions();

  if (permisos.receive === "granted") {
    return true;
  }

  const resultado = await PushNotifications.requestPermissions();

  return resultado.receive === "granted";
}

export async function registrar_push(): Promise<void> {
  const autorizado = await solicitar_permisos_push();

  if (!autorizado) {
    throw new Error("No se otorgaron permisos para recibir notificaciones.");
  }

  registrar_listeners();

  await PushNotifications.register();
}

function registrar_listeners(): void {
  if (listeners_registrados) {
    return;
  }

  listeners_registrados = true;

  PushNotifications.addListener("registration", async (token: Token) => {
    console.log("Token FCM recibido:", token.value);

    try {
      await registrar_dispositivo(token.value, "android");

      console.log("Dispositivo registrado correctamente.");
    } catch (error) {
      console.error("No se pudo registrar el dispositivo:", error);
    }
  });

  PushNotifications.addListener("registrationError", (error) => {
    console.error("Error registrando Push Notifications:", error);
  });

  PushNotifications.addListener(
    "pushNotificationReceived",
    (notification: PushNotificationSchema) => {
      console.log("Notificación push recibida:", notification);
    },
  );

  PushNotifications.addListener(
    "pushNotificationActionPerformed",
    (action: ActionPerformed) => {
      console.log("Acción sobre notificación push:", action);
    },
  );
}
