import {
  obtener,
  obtener_blob,
  crear,
  actualizar,
  eliminar,
  ApiError,
  crear_form_data,
} from "@/services/ajax_service";
import { obtener_api_url } from "@/config/debug";

import type { Lote, EstadoLote } from "@/models/LoteModel";

import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";

interface ProveedorApi {
  id: number;
  nombre: string;
  descripcion: string;
  verificado: boolean;
  latitud?: number | null;
  longitud?: number | null;
}

interface LoteApi {
  id: number;
  nombre: string;
  descripcion: string;
  categoria: string;
  precio_mercado: number;
  precio_cupo: number;
  porcentaje_ahorro: number;
  cantidad_cupos: number;
  cupos_ocupados: number;
  fecha_inicio: string;
  fecha_fin: string;
  estado: EstadoLote;
  imagen_avatar?: string | null;
  proveedor_id?: number;
  proveedor?: ProveedorApi | null;
}

interface LoteResponseApi {
  id: number;
  nombre: string;
  descripcion: string;
  categoria: string;
  precio_mercado: number;
  precio_cupo: number;
  porcentaje_ahorro: number;
  cantidad_cupos: number;
  cupos_ocupados: number;
  fecha_inicio: string;
  fecha_fin: string;
  estado: EstadoLote;
  imagen_avatar?: string | null;
  proveedor_id: number;
  proveedor?: ProveedorApi;
}

interface LoteDetalleApi {
  id: number;
  nombre: string;
  descripcion: string;
  categoria: string;
  precioMercado: number;
  precioCupo: number;
  porcentajeAhorro: number;
  cantidadCupos: number;
  cuposOcupados: number;
  fechaInicio: string;
  fechaFin: string;
  estado: EstadoLote;
  imagenAvatar?: string | null;
  proveedorId: number;
  proveedor?: ProveedorApi | null;
}

function mapearLoteDetalle(lote: LoteDetalleApi): Lote {
  const proveedor = mapearProveedorLote(lote.proveedor, lote.proveedorId);

  return {
    id: lote.id,
    nombre: lote.nombre,
    descripcion: lote.descripcion,
    categoria: lote.categoria,
    precioMercado: lote.precioMercado,
    precioCupo: lote.precioCupo,
    porcentajeAhorro: lote.porcentajeAhorro,
    cantidadCupos: lote.cantidadCupos,
    cuposOcupados: lote.cuposOcupados,
    fechaInicio: lote.fechaInicio,
    fechaFin: lote.fechaFin,
    estado: lote.estado,
    imagenAvatar: lote.imagenAvatar ?? null,
    proveedorId: lote.proveedorId,
    proveedor: mapearProveedor(proveedor),
  };
}

interface RespuestaApi<T> {
  exito: boolean;
  mensaje: string;
  datos: T | null;
  errores: string[];
}

interface ResultadoPaginado<T> {
  datos: T[];
  page: number;
  limit: number;
  offset: number;
  total: number;
  totalPaginas: number;
}

interface LotesPaginadosApi {
  datos: LoteApi[];
  page: number;
  limit: number;
  offset: number;
  total: number;
  totalPaginas: number;
}

function mapearProveedor(proveedor: ProveedorApi): ProveedorApi {
  return {
    id: proveedor.id,
    nombre: proveedor.nombre,
    descripcion: proveedor.descripcion,
    verificado: proveedor.verificado,
    latitud: proveedor.latitud ?? null,
    longitud: proveedor.longitud ?? null,
  };
}

function mapearProveedorLote(
  proveedor: ProveedorApi | null | undefined,
  proveedor_id: number | undefined,
): ProveedorApi {
  const id = proveedor?.id && proveedor.id > 0 ? proveedor.id : proveedor_id;
  if (!Number.isInteger(id) || id === undefined || id <= 0) {
    throw new Error("La API devolvió un lote sin un proveedor válido.");
  }
  const proveedor_valido = proveedor?.id === id ? proveedor : {
    id,
    nombre: `Proveedor #${id}`,
    descripcion: "",
    verificado: false,
  };
  return mapearProveedor(proveedor_valido);
}

function mapearLote(lote: LoteApi): Lote {
  const proveedor = mapearProveedorLote(lote.proveedor, lote.proveedor_id);

  return {
    id: lote.id,
    nombre: lote.nombre,
    descripcion: lote.descripcion,
    categoria: lote.categoria,
    precioMercado: lote.precio_mercado,
    precioCupo: lote.precio_cupo,
    porcentajeAhorro: lote.porcentaje_ahorro,
    cantidadCupos: lote.cantidad_cupos,
    cuposOcupados: lote.cupos_ocupados,
    fechaInicio: lote.fecha_inicio,
    fechaFin: lote.fecha_fin,
    estado: lote.estado,
    imagenAvatar: lote.imagen_avatar ?? null,
    proveedorId: proveedor.id,
    proveedor: mapearProveedor(proveedor),
  };
}

function mapearRespuesta(lote: LoteResponseApi): Lote {
  const proveedor = mapearProveedorLote(lote.proveedor, lote.proveedor_id);

  return {
    id: lote.id,
    nombre: lote.nombre,
    descripcion: lote.descripcion,
    categoria: lote.categoria,
    precioMercado: lote.precio_mercado,
    precioCupo: lote.precio_cupo,
    porcentajeAhorro: lote.porcentaje_ahorro,
    cantidadCupos: lote.cantidad_cupos,
    cuposOcupados: lote.cupos_ocupados,
    fechaInicio: lote.fecha_inicio,
    fechaFin: lote.fecha_fin,
    estado: lote.estado,
    imagenAvatar: lote.imagen_avatar ?? null,
    proveedorId: lote.proveedor_id,
    proveedor: mapearProveedor(proveedor),
  };
}

function obtenerDatos<T>(respuesta: RespuestaApi<T>): T {
  if (!respuesta.exito || respuesta.datos === null) {
    throw new Error(
      respuesta.errores.join(" ") ||
        respuesta.mensaje ||
        "La API devolvió una respuesta inválida.",
    );
  }

  return respuesta.datos;
}

export function url_imagen_lote(imagen: string | null | undefined): string | null {
  if (!imagen) return null;

  const apiUrl = obtener_api_url();
  if (!apiUrl) return null;

  try {
    const apiOrigin = new URL(apiUrl).origin;
    const url = new URL(imagen, `${apiOrigin}/`);
    if (url.protocol === "https:" || url.origin === apiOrigin) {
      return url.href;
    }
  } catch {
    return null;
  }

  return null;
}

function validarTexto(valor: string, campo: string): void {
  if (!valor || !valor.trim()) {
    throw new Error(`El campo ${campo} es obligatorio.`);
  }
}

function validarNumeroPositivo(valor: number, campo: string): void {
  if (!Number.isFinite(valor) || valor <= 0) {
    throw new Error(`El campo ${campo} debe ser mayor a 0.`);
  }
}

function validarRequestLote(
  request: CrearLoteRequest | ActualizarLoteRequest,
): void {
  validarTexto(request.nombre, "nombre");
  validarTexto(request.descripcion, "descripción");
  validarTexto(request.categoria, "categoría");

  validarNumeroPositivo(request.precioMercado, "precio de mercado");

  validarNumeroPositivo(request.precioCupo, "precio de cupo");

  if (!Number.isInteger(request.cantidadCupos) || request.cantidadCupos <= 0) {
    throw new Error(
      "La cantidad de cupos debe ser un número entero mayor a 0.",
    );
  }

  if (!request.fechaInicio) {
    throw new Error("La fecha de inicio es obligatoria.");
  }

  if (!request.fechaFin) {
    throw new Error("La fecha de finalización es obligatoria.");
  }

  const fecha_inicio = new Date(`${request.fechaInicio}T00:00:00`);

  const fecha_fin = new Date(`${request.fechaFin}T00:00:00`);

  if (Number.isNaN(fecha_inicio.getTime())) {
    throw new Error("La fecha de inicio no es válida.");
  }

  if (Number.isNaN(fecha_fin.getTime())) {
    throw new Error("La fecha de finalización no es válida.");
  }

  if (fecha_fin <= fecha_inicio) {
    throw new Error(
      "La fecha de finalización debe ser posterior a la fecha de inicio.",
    );
  }

  if (!Number.isInteger(request.proveedorId) || request.proveedorId <= 0) {
    throw new Error("El proveedor seleccionado no es válido.");
  }

  if (request.precioCupo >= request.precioMercado) {
    throw new Error("El precio de cupo debe ser menor al precio de mercado.");
  }
}

function manejarError(error: unknown): never {
  if (error instanceof ApiError) {
    if (error.status === 502 && error.message.trim()) {
      throw new Error(error.message);
    }

    if (error.status >= 500) {
      throw new Error(
        "Ocurrió un error interno en el servidor. Intentá nuevamente.",
      );
    }

    throw error;
  }

  if (error instanceof Error) {
    throw error;
  }

  throw new Error("Ocurrió un error inesperado. Intentá nuevamente.");
}

export async function obtener_lotes(
  page = 1,
  limit = 10,
  termino?: string,
  sort?: string,
): Promise<ResultadoPaginado<Lote>> {
  const parametros = new URLSearchParams();

  parametros.set("page", String(page));
  parametros.set("limit", String(limit));

  if (termino?.trim()) {
    parametros.set("termino", termino.trim());
  }

  if (sort) {
    parametros.set("sort", sort);
  }

  try {
    const respuesta = await obtener<RespuestaApi<LotesPaginadosApi>>(
      `lotes?${parametros.toString()}`,
    );

    const datos = obtenerDatos(respuesta);

    return {
      datos: datos.datos.map(mapearLote),
      page: datos.page,
      limit: datos.limit,
      offset: datos.offset,
      total: datos.total,
      totalPaginas: datos.totalPaginas,
    };
  } catch (error) {
    return manejarError(error);
  }
}

export async function obtener_lote(id: number): Promise<Lote> {
  try {
    const respuesta = await obtener<RespuestaApi<LoteDetalleApi>>(
      `lotes/${id}`,
    );

    return mapearLoteDetalle(obtenerDatos(respuesta));
  } catch (error) {
    return manejarError(error);
  }
}

export async function crear_lote(request: CrearLoteRequest): Promise<Lote> {
  validarRequestLote(request);

  try {
    const respuesta = await crear<RespuestaApi<LoteResponseApi>>(
      "lotes",
      request,
    );

    return mapearRespuesta(obtenerDatos(respuesta));
  } catch (error) {
    return manejarError(error);
  }
}

interface RespuestaImagenLote {
  imagen_avatar: string;
}

export async function subir_imagen_lote(imagen: File): Promise<string> {
  const formData = new FormData();
  formData.append("imagen", imagen, imagen.name);

  try {
    const respuesta = await crear_form_data<RespuestaApi<RespuestaImagenLote>>(
      "lotes/imagen-avatar",
      formData,
    );
    const imagen_avatar = obtenerDatos(respuesta).imagen_avatar;
    if (typeof imagen_avatar !== "string" || !imagen_avatar.trim()) {
      throw new Error("La API no devolvió la ruta de la imagen subida.");
    }
    return imagen_avatar;
  } catch (error) {
    return manejarError(error);
  }
}

export async function actualizar_lote(
  id: number,
  request: ActualizarLoteRequest,
): Promise<Lote> {
  if (!Number.isInteger(id) || id <= 0) {
    throw new Error("El identificador del lote no es válido.");
  }

  validarRequestLote(request);

  try {
    const respuesta = await actualizar<RespuestaApi<LoteResponseApi>>(
      `lotes/${id}`,
      request,
    );

    return mapearRespuesta(obtenerDatos(respuesta));
  } catch (error) {
    return manejarError(error);
  }
}

export async function eliminar_lote(id: number): Promise<void> {
  if (!Number.isInteger(id) || id <= 0) {
    throw new Error("El identificador del lote no es válido.");
  }

  try {
    await eliminar(`lotes/${id}`);
  } catch (error) {
    return manejarError(error);
  }
}

export async function obtener_pdf_lote(id: number): Promise<Blob> {
  if (!Number.isInteger(id) || id <= 0) {
    throw new Error("El identificador del lote no es válido.");
  }

  try {
    return await obtener_blob(`lotes/${id}/pdf`);
  } catch (error) {
    return manejarError(error);
  }
}

export async function obtener_qr_lote(id: number): Promise<Blob> {
  if (!Number.isInteger(id) || id <= 0) {
    throw new Error("El identificador del lote no es válido.");
  }

  try {
    return await obtener_blob(`lotes/${id}/qr`);
  } catch (error) {
    return manejarError(error);
  }
}

interface CambiarEstadoLoteResponse {
  exito: boolean;
  mensaje: string;
  datos: {
    id: number;
    estado: string;
  } | null;
  errores: string[];
}

export async function cambiar_estado_lote(
  lote_id: number,
  estado: string,
): Promise<CambiarEstadoLoteResponse> {
  return actualizar<CambiarEstadoLoteResponse>(`lotes/${lote_id}/estado`, {
    estado,
  });
}
