const API_URL = import.meta.env.VITE_API_URL ?? "";

export function obtener_api_url(): string {
  return API_URL.replace(/\/+$/, "");
}
