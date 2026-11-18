import { Capacitor } from "@capacitor/core";
import { Directory, Filesystem } from "@capacitor/filesystem";
import { obtener_api_url } from "@/config/debug";

function clave(usuario_id: string): string { return `syner_perfil_foto:${usuario_id}`; }

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
