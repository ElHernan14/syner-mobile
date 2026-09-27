import { Haptics, ImpactStyle, NotificationType } from "@capacitor/haptics";

export async function vibrar(duracion = 300): Promise<void> {
  await Haptics.vibrate({
    duration: duracion,
  });
}

export async function vibrar_exito(): Promise<void> {
  await Haptics.notification({
    type: NotificationType.Success,
  });
}

export async function vibrar_error(): Promise<void> {
  await Haptics.notification({
    type: NotificationType.Error,
  });
}

export async function vibrar_impacto(): Promise<void> {
  await Haptics.impact({
    style: ImpactStyle.Medium,
  });
}
