import {
  Camera,
  MediaTypeSelection,
  type MediaResult,
} from "@capacitor/camera";

export interface FotoSeleccionada {
  uri: string | undefined;
  web_path: string | undefined;
  formato: string;
  nombre: string;
}

function convertir_resultado(resultado: MediaResult): FotoSeleccionada {
  const formato = resultado.metadata?.format ?? "jpeg";
  const extension = formato === "jpg" ? "jpg" : "jpeg";

  return {
    uri: resultado.uri,
    web_path: resultado.webPath,
    formato,
    nombre: `foto.${extension}`,
  };
}

export async function sacar_foto(): Promise<FotoSeleccionada> {
  const resultado = await Camera.takePhoto({
    quality: 90,
    targetWidth: 1200,
    targetHeight: 1200,
    correctOrientation: true,
    includeMetadata: true,
  });

  return convertir_resultado(resultado);
}

export async function elegir_foto_galeria(): Promise<FotoSeleccionada> {
  const { results } = await Camera.chooseFromGallery({
    mediaType: MediaTypeSelection.Photo,
    allowMultipleSelection: false,
    quality: 90,
    targetWidth: 1200,
    targetHeight: 1200,
    includeMetadata: true,
  });

  if (results.length === 0) {
    throw new Error("No se seleccionó ninguna imagen.");
  }

  return convertir_resultado(results[0]);
}
