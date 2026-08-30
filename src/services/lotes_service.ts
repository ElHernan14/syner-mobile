import { obtener } from "@/services/ajax_service";
import type { Lote } from "@/data/lote";

export function obtener_lotes(): Promise<Lote[]> {
  return obtener<Lote[]>("lotes");
}
