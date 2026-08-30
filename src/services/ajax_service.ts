import { obtener_api_url } from "@/config/debug";

const api_url = obtener_api_url();

function construir_url(endpoint: string): string {
  if (!api_url) {
    throw new Error("No se configuró la URL de la API.");
  }

  return `${api_url}/${String(endpoint).replace(/^\/+/, "")}`;
}

export async function obtener<T>(endpoint: string): Promise<T> {
  const response = await fetch(construir_url(endpoint));

  if (!response.ok) {
    throw new Error(`Error al consultar la API. Código: ${response.status}`);
  }

  return response.json() as Promise<T>;
}
