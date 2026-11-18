import { Geolocation, type Position } from "@capacitor/geolocation";

export async function solicitar_permisos_ubicacion(): Promise<boolean> {
  const permisos = await Geolocation.checkPermissions();

  if (
    permisos.location === "granted" ||
    permisos.coarseLocation === "granted"
  ) {
    return true;
  }

  const resultado = await Geolocation.requestPermissions();

  return (
    resultado.location === "granted" || resultado.coarseLocation === "granted"
  );
}

export async function obtener_ubicacion_actual(): Promise<Position> {
  let autorizado: boolean;
  try {
    autorizado = await solicitar_permisos_ubicacion();
  } catch (error) {
    throw traducir_error_ubicacion(error);
  }

  if (!autorizado) {
    throw new Error("No se otorgaron permisos para acceder a la ubicación.");
  }

  try {
    return await Geolocation.getCurrentPosition({
      enableHighAccuracy: true,
      timeout: 10000,
      maximumAge: 30000,
    });
  } catch (error) {
    throw traducir_error_ubicacion(error);
  }
}

function traducir_error_ubicacion(error: unknown): Error {
  const texto = error instanceof Error ? `${error.name} ${error.message}`.toLowerCase() : String(error).toLowerCase();
  if (texto.includes("permission") || texto.includes("denied") || texto.includes("permiso")) return new Error("No se concedió permiso para acceder a la ubicación. Podés habilitarlo en los ajustes del teléfono.");
  if (texto.includes("timeout") || texto.includes("timed out")) return new Error("La ubicación está tardando más de lo esperado. Revisá que el GPS esté activado e intentá de nuevo.");
  if (texto.includes("unavailable") || texto.includes("location services") || texto.includes("disabled")) return new Error("No se pudo obtener la ubicación. Activá el GPS y volvé a intentarlo.");
  return new Error("No se pudo obtener la ubicación en este momento. Revisá el GPS y los permisos de la app.");
}
