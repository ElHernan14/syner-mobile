import { beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/config/debug", () => ({
  obtener_api_url: () => "https://api.example.test",
}));

vi.mock("@/services/ajax_service", () => ({
  obtener: vi.fn(),
  obtener_blob: vi.fn(),
  crear: vi.fn(),
  crear_form_data: vi.fn(),
  actualizar: vi.fn(),
  eliminar: vi.fn(),
  ApiError: class ApiError extends Error {
    constructor(
      message: string,
      readonly status: number,
      readonly errores: string[] = [],
    ) {
      super(message);
      this.name = "ApiError";
    }
  },
}));

import {
  ApiError,
  actualizar as actualizar_api,
  crear as crear_api,
  crear_form_data,
} from "@/services/ajax_service";
import {
  actualizar_lote,
  crear_lote,
  subir_imagen_lote,
} from "@/services/lotes_service";
import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";

const request_base = {
  nombre: "Lote de prueba",
  descripcion: "Descripción de prueba",
  categoria: "tecnologia",
  precioMercado: 1000,
  precioCupo: 800,
  cantidadCupos: 10,
  fechaInicio: "2026-11-26",
  fechaFin: "2026-12-26",
  proveedorId: 7,
};

function respuesta_lote(proveedor_id: number) {
  return {
    exito: true,
    mensaje: "Correcto",
    errores: [],
    datos: {
      id: 42,
      nombre: request_base.nombre,
      descripcion: request_base.descripcion,
      categoria: request_base.categoria,
      precio_mercado: request_base.precioMercado,
      precio_cupo: request_base.precioCupo,
      porcentaje_ahorro: 20,
      cantidad_cupos: request_base.cantidadCupos,
      cupos_ocupados: 0,
      fecha_inicio: request_base.fechaInicio,
      fecha_fin: request_base.fechaFin,
      estado: "borrador",
      imagen_avatar: null,
      proveedor_id,
      proveedor: null,
    },
  };
}

describe("lotes_service create/edit and image upload", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("creates a lot without an image when the response has no nested provider", async () => {
    vi.mocked(crear_api).mockResolvedValue(respuesta_lote(7));

    const request: CrearLoteRequest = {
      ...request_base,
      imagenAvatar: null,
    };
    const lote = await crear_lote(request);

    expect(crear_api).toHaveBeenCalledWith("lotes", request);
    expect(lote.id).toBe(42);
    expect(lote.proveedor.id).toBe(7);
    expect(lote.proveedor.nombre).toBe("Proveedor #7");
  });

  it("updates a lot when the response omits the nested provider", async () => {
    vi.mocked(actualizar_api).mockResolvedValue(respuesta_lote(9));

    const request: ActualizarLoteRequest = {
      ...request_base,
      proveedorId: 9,
      imagenAvatar: null,
    };
    const lote = await actualizar_lote(42, request);

    expect(actualizar_api).toHaveBeenCalledWith("lotes/42", request);
    expect(lote.id).toBe(42);
    expect(lote.proveedor.id).toBe(9);
    expect(lote.proveedor.nombre).toBe("Proveedor #9");
  });

  it("uploads the selected image and returns the storage URL", async () => {
    vi.mocked(crear_form_data).mockResolvedValue({
      exito: true,
      mensaje: "Correcto",
      errores: [],
      datos: { imagen_avatar: "https://storage.example.test/lotes/lote.webp" },
    });

    const image = new File(["webp"], "lote.webp", { type: "image/webp" });
    const url = await subir_imagen_lote(image);
    const [, form_data] = vi.mocked(crear_form_data).mock.calls[0];

    expect(url).toBe("https://storage.example.test/lotes/lote.webp");
    expect(form_data.get("imagen")).toMatchObject({
      name: "lote.webp",
      type: "image/webp",
    });
  });

  it("shows the safe storage error returned by the API", async () => {
    vi.mocked(crear_form_data).mockRejectedValue(
      new ApiError(
        "El almacenamiento de imágenes no está disponible.",
        502,
        ["El almacenamiento de imágenes no está disponible."],
      ),
    );

    const image = new File(["webp"], "lote.webp", { type: "image/webp" });

    await expect(subir_imagen_lote(image)).rejects.toThrow(
      "El almacenamiento de imágenes no está disponible.",
    );
  });
});
