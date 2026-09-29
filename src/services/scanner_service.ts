import {
  CapacitorBarcodeScanner,
  CapacitorBarcodeScannerCameraDirection,
  CapacitorBarcodeScannerScanOrientation,
  CapacitorBarcodeScannerTypeHint,
} from "@capacitor/barcode-scanner";

export interface ResultadoEscaneo {
  contenido: string;
}

export async function escanear_qr(): Promise<ResultadoEscaneo> {
  const resultado = await CapacitorBarcodeScanner.scanBarcode({
    hint: CapacitorBarcodeScannerTypeHint.QR_CODE,
    scanInstructions: "Apuntá la cámara al código QR del lote",
    scanButton: true,
    scanText: "Escanear",
    cameraDirection: CapacitorBarcodeScannerCameraDirection.BACK,
    scanOrientation: CapacitorBarcodeScannerScanOrientation.PORTRAIT,
  });

  const contenido = resultado.ScanResult?.trim();

  if (!contenido) {
    throw new Error("No se obtuvo ningún contenido del código QR.");
  }

  return {
    contenido,
  };
}
