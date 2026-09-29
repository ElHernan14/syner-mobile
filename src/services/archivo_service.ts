import { Directory, Filesystem } from "@capacitor/filesystem";
import { Share } from "@capacitor/share";
import { FileOpener } from "@capawesome-team/capacitor-file-opener";

function blob_a_base64(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const lector = new FileReader();

    lector.onloadend = () => {
      const resultado = lector.result;

      if (typeof resultado !== "string") {
        reject(new Error("No se pudo convertir el archivo."));
        return;
      }

      const base64 = resultado.split(",")[1];

      if (!base64) {
        reject(new Error("El archivo no contiene datos válidos."));
        return;
      }

      resolve(base64);
    };

    lector.onerror = () => {
      reject(new Error("No se pudo leer el archivo."));
    };

    lector.readAsDataURL(blob);
  });
}

async function guardar_archivo_temporal(
  blob: Blob,
  nombre_archivo: string,
): Promise<string> {
  if (blob.size === 0) {
    throw new Error("El archivo está vacío.");
  }

  const base64 = await blob_a_base64(blob);

  const archivo = await Filesystem.writeFile({
    path: nombre_archivo,
    data: base64,
    directory: Directory.Cache,
    recursive: true,
  });

  return archivo.uri;
}

export async function abrir_pdf(
  blob: Blob,
  nombre_archivo: string,
): Promise<void> {
  if (blob.type !== "application/pdf") {
    throw new Error("El archivo recibido no es un PDF válido.");
  }

  const uri = await guardar_archivo_temporal(blob, nombre_archivo);

  await FileOpener.openFile({
    path: uri,
    mimeType: "application/pdf",
  });
}

export async function compartir_pdf(
  blob: Blob,
  nombre_archivo: string,
): Promise<void> {
  const uri = await guardar_archivo_temporal(blob, nombre_archivo);

  await Share.share({
    title: "Ficha del lote",
    text: "Ficha del lote SYNER",
    url: uri,
    dialogTitle: "Compartir ficha del lote",
  });
}
