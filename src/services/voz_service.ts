import { SpeechRecognition } from "@capacitor-community/speech-recognition";

export async function dictado_disponible(): Promise<boolean> {
  try {
    const resultado = await SpeechRecognition.available();
    return resultado.available;
  } catch {
    return false;
  }
}

export async function dictar_texto_largo(): Promise<string> {
  const permisos = await SpeechRecognition.checkPermissions();
  if (permisos.speechRecognition !== "granted") {
    const solicitado = await SpeechRecognition.requestPermissions();
    if (solicitado.speechRecognition !== "granted") {
      throw new Error("Necesitamos permiso para usar el micrófono. Podés seguir escribiendo con el teclado.");
    }
  }

  const resultado = await SpeechRecognition.start({
    language: "es-AR",
    maxResults: 1,
    prompt: "Dictá la descripción del lote",
    popup: true,
    partialResults: false,
  });
  const texto = resultado.matches?.[0]?.trim();
  if (!texto) throw new Error("No se reconoció el dictado. Podés intentarlo de nuevo o escribir con el teclado.");
  return texto;
}
