import { Capacitor } from "@capacitor/core";
import { Directory, Filesystem } from "@capacitor/filesystem";
import { obtener_api_url } from "@/config/debug";

function clave(usuario_id: string): string { return `syner_perfil_foto:${usuario_id}`; }
function clave_pendiente(usuario_id: string): string { return `syner_perfil_foto_pendiente:${usuario_id}`; }

function blob_a_base64(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const lector = new FileReader();
    lector.onload = () => {
      const valor = String(lector.result ?? "").split(",")[1];
      valor ? resolve(valor) : reject(new Error("No se pudo leer la imagen seleccionada."));
    };
    lector.onerror = () => reject(new Error("No se pudo leer la imagen seleccionada."));
    lector.readAsDataURL(blob);
  });
}

export async function guardar_foto_local(blob: Blob, usuario_id: string): Promise<string> {
  const extension = blob.type.includes("png") ? "png" : blob.type.includes("webp") ? "webp" : "jpg";
  const path = `perfil/${usuario_id}.${extension}`;
  await Filesystem.writeFile({ path, data: await blob_a_base64(blob), directory: Directory.Data, recursive: true });
  localStorage.setItem(clave(usuario_id), path);
  return (await cargar_url_foto_local(usuario_id)) ?? "";
}

export async function encolar_foto_perfil(blob: Blob, usuario_id: string, nombre: string): Promise<void> {
  const extension = blob.type.includes("png") ? "png" : blob.type.includes("webp") ? "webp" : "jpg";
  const path = `perfil/${usuario_id}.${extension}`;
  await Filesystem.writeFile({ path, data: await blob_a_base64(blob), directory: Directory.Data, recursive: true });
  localStorage.setItem(clave(usuario_id), path);
  localStorage.setItem(clave_pendiente(usuario_id), JSON.stringify({ path, nombre, tipo: blob.type || "image/jpeg" }));
}

export async function obtener_foto_perfil_pendiente(usuario_id: string): Promise<{ archivo: File; nombre: string } | null> {
  const raw = localStorage.getItem(clave_pendiente(usuario_id));
  if (!raw) return null;
  try {
    const pendiente = JSON.parse(raw) as { path: string; nombre: string; tipo: string };
    const archivo_local = await Filesystem.readFile({ path: pendiente.path, directory: Directory.Data });
    const base64 = typeof archivo_local.data === "string" ? archivo_local.data : "";
    const binario = atob(base64);
    const bytes = Uint8Array.from(binario, (caracter) => caracter.charCodeAt(0));
    return { archivo: new File([bytes], pendiente.nombre, { type: pendiente.tipo }), nombre: pendiente.nombre };
  } catch {
    localStorage.removeItem(clave_pendiente(usuario_id));
    return null;
  }
}

export function limpiar_foto_perfil_pendiente(usuario_id: string): void {
  localStorage.removeItem(clave_pendiente(usuario_id));
}

export async function cargar_url_foto_local(usuario_id?: string | null): Promise<string | null> {
  if (!usuario_id) return null;
  const path = localStorage.getItem(clave(usuario_id));
  if (!path) return null;
  try {
    const { uri } = await Filesystem.getUri({ path, directory: Directory.Data });
    return Capacitor.convertFileSrc(uri);
  } catch {
    localStorage.removeItem(clave(usuario_id));
    return null;
  }
}

export function url_avatar_servidor(avatar?: string | null): string | null {
  if (!avatar) return null;
  if (/^https?:\/\//i.test(avatar)) return avatar;
  const api = obtener_api_url().replace(/\/+$/, "");
  const origen = api.replace(/\/api$/i, "");
  return `${origen}/${avatar.replace(/^\/+/, "")}`;
}
