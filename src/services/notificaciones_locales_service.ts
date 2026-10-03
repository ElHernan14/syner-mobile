import {
  LocalNotifications,
  type PermissionStatus,
} from "@capacitor/local-notifications";

export async function solicitar_permisos_notificaciones(): Promise<boolean> {
  const permisos: PermissionStatus =
    await LocalNotifications.checkPermissions();

  if (permisos.display === "granted") {
    return true;
  }

  const resultado = await LocalNotifications.requestPermissions();

  return resultado.display === "granted";
}

export async function mostrar_notificacion_local(
  titulo: string,
  cuerpo: string,
): Promise<void> {
  const autorizado = await solicitar_permisos_notificaciones();

  if (!autorizado) {
    throw new Error("No se otorgaron permisos para mostrar notificaciones.");
  }

  await LocalNotifications.schedule({
    notifications: [
      {
        id: Date.now(),
        title: titulo,
        body: cuerpo,
        schedule: {
          at: new Date(Date.now() + 1000),
        },
      },
    ],
  });
}
