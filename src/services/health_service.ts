import { obtener } from "@/services/ajax_service";

interface HealthApi {
  estado: string;
  mensaje: string;
}

export async function obtener_health(): Promise<HealthApi> {
  return obtener<HealthApi>("health");
}
