import {
  PushNotifications,
  type ActionPerformed,
  type PushNotificationSchema,
  type Token,
} from "@capacitor/push-notifications";
import { LocalNotifications } from "@capacitor/local-notifications";
import { Capacitor } from "@capacitor/core";

import { registrar_dispositivo } from "@/services/dispositivos_service";
import { desactivar_dispositivo } from "@/services/dispositivos_service";
import { mostrar_notificacion_local } from "@/services/notificaciones_locales_service";
import router from "@/router";

let listeners_registrados = false;
const PUSH_TOKEN_KEY = "syner_push_token";

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

  if (Capacitor.getPlatform() === "android") {
    await LocalNotifications.createChannel({
      id: "syner_high_importance",
      name: "Notificaciones de SYNER",
      description: "Novedades sobre lotes y pedidos.",
      importance: 5,
      sound: "default",
      vibration: true,
    });
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
    try {
      await registrar_dispositivo(token.value, "android");
      localStorage.setItem(PUSH_TOKEN_KEY, token.value);
    } catch (error) {
      console.error("No se pudo registrar el dispositivo:", error);
    }
  });

  PushNotifications.addListener("registrationError", (error) => {
    console.error("Error registrando Push Notifications:", error);
  });

  PushNotifications.addListener(
    "pushNotificationReceived",
    async (notification: PushNotificationSchema) => {
      await mostrar_notificacion_local(
        notification.title ?? "SYNER",
        notification.body ?? "Tenés una actualización.",
        normalizar_datos(notification.data),
      );
    },
  );

  PushNotifications.addListener(
    "pushNotificationActionPerformed",
    (action: ActionPerformed) => {
      navegar_desde_notificacion(normalizar_datos(action.notification.data));
    },
  );

  LocalNotifications.addListener(
    "localNotificationActionPerformed",
    (action) => {
      navegar_desde_notificacion(normalizar_datos(action.notification.extra));
    },
  );
}

export async function desactivar_push(): Promise<void> {
  const token = localStorage.getItem(PUSH_TOKEN_KEY);

  if (!token) {
    return;
  }

  await desactivar_dispositivo(token);
  // Si el servidor no confirma la baja, conservamos el token para poder
  // reintentarla en el siguiente inicio de sesión.
  if (localStorage.getItem(PUSH_TOKEN_KEY) === token) {
    localStorage.removeItem(PUSH_TOKEN_KEY);
  }
}

function normalizar_datos(datos: unknown): Record<string, string> {
  if (!datos || typeof datos !== "object") {
    return {};
  }

  return Object.fromEntries(
    Object.entries(datos).filter(
      ([, valor]) => typeof valor === "string",
    ) as [string, string][],
  );
}

function navegar_desde_notificacion(datos: Record<string, string>): void {
  const entidad_tipo = datos.entidadTipo;
  const entidad_id = Number(datos.entidadId);

  if (!Number.isInteger(entidad_id) || entidad_id <= 0) {
    void router.push({ name: "notificaciones" });
    return;
  }

  if (entidad_tipo === "lote") {
    void router.push({ name: "lote-detalle", params: { id: entidad_id } });
    return;
  }

  void router.push({ name: "pedidos", query: { pedidoId: entidad_id } });
}
