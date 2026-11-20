const API_URL = import.meta.env.DEV
  ? (import.meta.env.VITE_API_URL_DEBUG ?? import.meta.env.VITE_API_URL ?? "")
  : (import.meta.env.VITE_API_URL ?? "https://syner-api.runasp.net/api");

export function obtener_api_url(): string {
  return API_URL.replace(/\/+$/, "");
}
