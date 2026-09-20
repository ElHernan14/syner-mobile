import { obtener_api_url } from "@/config/debug";

import { useAuthStore } from "@/stores/auth_store";

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

function es_endpoint_auth(endpoint: string): boolean {
  const endpoint_normalizado = endpoint.replace(/^\/+/, "").toLowerCase();

  return (
    endpoint_normalizado === "auth/login" ||
    endpoint_normalizado === "auth/refresh" ||
    endpoint_normalizado === "auth/logout"
  );
}

let refresh_en_curso: Promise<void> | null = null;

async function ejecutar_refresh(): Promise<void> {
  const auth_store = useAuthStore();

  if (!auth_store.refresh_token) {
    throw new Error("No existe un refresh token.");
  }

  if (!refresh_en_curso) {
    refresh_en_curso = auth_store
      .refrescar_sesion()
      .then(() => undefined)
      .finally(() => {
        refresh_en_curso = null;
      });
  }

  await refresh_en_curso;
}

async function ejecutar_request(
  endpoint: string,
  opciones: RequestInit,
): Promise<Response> {
  const auth_store = useAuthStore();

  const headers = new Headers(opciones.headers);

  headers.set("Content-Type", "application/json");

  if (auth_store.access_token) {
    headers.set("Authorization", `Bearer ${auth_store.access_token}`);
  } else {
    headers.delete("Authorization");
  }

  try {
    return await fetch(construir_url(endpoint), {
      ...opciones,
      headers,
    });
  } catch {
    throw new Error("No se pudo establecer conexión con la API.");
  }
}

async function ejecutar<T>(
  endpoint: string,
  opciones: RequestInit,
): Promise<T> {
  let response = await ejecutar_request(endpoint, opciones);

  if (response.status === 401 && !es_endpoint_auth(endpoint)) {
    try {
      console.log("token expirado ! - Se ejecuta refresh");
      await ejecutar_refresh();

      response = await ejecutar_request(endpoint, opciones);
    } catch {
      throw new ApiError("La sesión expiró. Iniciá sesión nuevamente.", 401);
    }
  }

  if (!response.ok) {
    throw await obtener_error_api(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  console.log("token no expirado !");
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
