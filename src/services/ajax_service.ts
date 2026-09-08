import { obtener_api_url } from "@/config/debug";

const api_url = obtener_api_url();

export class ApiError extends Error {
  readonly status: number;
  readonly errores: string[];

  constructor(mensaje: string, status: number, errores: string[] = []) {
    super(mensaje);
    this.name = "ApiError";
    this.status = status;
    this.errores = errores;
  }
}

function construir_url(endpoint: string): string {
  if (!api_url) {
    throw new Error("No se configuró la URL de la API.");
  }

  return `${api_url}/${String(endpoint).replace(/^\/+/, "")}`;
}

interface RespuestaErrorApi {
  exito?: boolean;
  mensaje?: string;
  errores?: string[];
}

async function obtener_error_api(response: Response): Promise<ApiError> {
  const status = response.status;

  try {
    const datos = (await response.json()) as RespuestaErrorApi;

    const errores = Array.isArray(datos.errores)
      ? datos.errores.filter(
          (error): error is string => typeof error === "string",
        )
      : [];

    const mensaje =
      errores.join(" ") ||
      (typeof datos.mensaje === "string" ? datos.mensaje : "") ||
      `Error al consultar la API. Código: ${status}`;

    return new ApiError(mensaje, status, errores);
  } catch {
    return new ApiError(`Error al consultar la API. Código: ${status}`, status);
  }
}

async function ejecutar<T>(
  endpoint: string,
  opciones: RequestInit,
): Promise<T> {
  let response: Response;

  try {
    response = await fetch(construir_url(endpoint), {
      ...opciones,
      headers: {
        "Content-Type": "application/json",
        ...(opciones.headers ?? {}),
      },
    });
  } catch {
    throw new Error("No se pudo establecer conexión con la API.");
  }

  if (!response.ok) {
    throw await obtener_error_api(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function obtener<T>(endpoint: string): Promise<T> {
  return ejecutar<T>(endpoint, {
    method: "GET",
  });
}

export async function crear<T>(endpoint: string, datos: unknown): Promise<T> {
  return ejecutar<T>(endpoint, {
    method: "POST",
    body: JSON.stringify(datos),
  });
}

export async function actualizar<T>(
  endpoint: string,
  datos: unknown,
): Promise<T> {
  return ejecutar<T>(endpoint, {
    method: "PUT",
    body: JSON.stringify(datos),
  });
}

export async function eliminar(endpoint: string): Promise<void> {
  await ejecutar<void>(endpoint, {
    method: "DELETE",
  });
}
