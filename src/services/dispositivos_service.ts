import { crear } from "@/services/ajax_service";

interface RegistrarDispositivoRequest {
  token: string;
  plataforma: string;
}

export interface DispositivoResponse {
  id: number;
  plataforma: string;
  activo: boolean;
  fechaRegistro: string;
}

interface RegistrarDispositivoApiResponse {
  exito: boolean;
  mensaje: string;
  datos: DispositivoResponse | null;
  errores: string[];
}

export async function registrar_dispositivo(
  token: string,
  plataforma: string,
): Promise<RegistrarDispositivoApiResponse> {
  const request: RegistrarDispositivoRequest = {
    token,
    plataforma,
  };

  return crear<RegistrarDispositivoApiResponse>("dispositivos", request);
}
