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
  datos: Record<string, string> = {},
): Promise<void> {
  const autorizado = await solicitar_permisos_notificaciones();

  if (!autorizado) {
    throw new Error("No se otorgaron permisos para mostrar notificaciones.");
  }

  // Android requires notification IDs to fit a signed 32-bit integer.
  const id_notificacion = Math.floor(Date.now() % 2_147_483_647) || 1;

  await LocalNotifications.schedule({
    notifications: [
      {
        id: id_notificacion,
        title: titulo,
        body: cuerpo,
        largeBody: `${cuerpo}\n\nAbrí SYNER para consultar el detalle y mantenerte al día.`,
        summaryText: "Novedades de lotes y pedidos · SYNER",
        group: "syner-actualizaciones",
        groupSummary: false,
        extra: datos,
        channelId: "syner_high_importance",
        smallIcon: "ic_stat_notificacion",
        iconColor: "#4968e8",
        schedule: {
          at: new Date(Date.now() + 1000),
        },
      },
    ],
  });
}
