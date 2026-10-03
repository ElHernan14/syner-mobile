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
  const autorizado = await solicitar_permisos_ubicacion();

  if (!autorizado) {
    throw new Error("No se otorgaron permisos para acceder a la ubicación.");
  }

  return Geolocation.getCurrentPosition({
    enableHighAccuracy: true,
    timeout: 10000,
    maximumAge: 30000,
  });
}
