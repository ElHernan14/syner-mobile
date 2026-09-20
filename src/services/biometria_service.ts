import {
  AccessControl,
  NativeBiometric,
} from "@capgo/capacitor-native-biometric";

const SERVER = "com.example.app";

export async function biometria_disponible(): Promise<boolean> {
  try {
    const resultado = await NativeBiometric.isAvailable();

    return resultado.isAvailable;
  } catch {
    return false;
  }
}

export async function guardar_refresh_token_biometria(
  refresh_token: string,
): Promise<void> {
  await NativeBiometric.setCredentials({
    username: "syner",
    password: refresh_token,
    server: SERVER,
    accessControl: AccessControl.BIOMETRY_CURRENT_SET,
  });
}

export async function obtener_refresh_token_biometria(): Promise<
  string | null
> {
  try {
    const resultado = await NativeBiometric.getSecureCredentials({
      server: SERVER,
      reason: "Autenticate para ingresar a SYNER.",
      title: "Ingresar a SYNER",
      subtitle: "Verificación biométrica",
      description: "Usá tu huella o rostro para recuperar tu sesión.",
    });

    return resultado.password || null;
  } catch {
    return null;
  }
}

export async function existe_sesion_biometrica(): Promise<boolean> {
  try {
    const resultado = await NativeBiometric.isCredentialsSaved({
      server: SERVER,
    });

    return resultado.isSaved;
  } catch {
    return false;
  }
}

export async function eliminar_sesion_biometrica(): Promise<void> {
  try {
    await NativeBiometric.deleteCredentials({
      server: SERVER,
    });
  } catch {
    // No hacemos fallar el logout si no existe una credencial biométrica.
  }
}
