import {
  obtener,
  crear,
  actualizar,
  eliminar,
  ApiError,
} from "@/services/ajax_service";

import type { Lote, EstadoLote } from "@/models/LoteModel";

import type { CrearLoteRequest } from "@/dtos/lotes/CrearLoteRequest";
import type { ActualizarLoteRequest } from "@/dtos/lotes/ActualizarLoteRequest";

interface ProveedorApi {
  id: number;
  nombre: string;
  descripcion: string;
  verificado: boolean;
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
  proveedor: ProveedorApi;
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
  proveedor_id: number;
  proveedor?: ProveedorApi;
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
  };
}

function mapearLote(lote: LoteApi): Lote {
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
    proveedorId: lote.proveedor.id,
    proveedor: mapearProveedor(lote.proveedor),
  };
}

function mapearRespuesta(lote: LoteResponseApi): Lote {
  const proveedor: ProveedorApi = lote.proveedor ?? {
    id: lote.proveedor_id,
    nombre: "",
    descripcion: "",
    verificado: false,
  };

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

  if (
    !Number.isFinite(request.porcentajeAhorro) ||
    request.porcentajeAhorro < 0 ||
    request.porcentajeAhorro > 100
  ) {
    throw new Error("El porcentaje de ahorro debe estar entre 0 y 100.");
  }

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
    const respuesta = await obtener<RespuestaApi<LoteResponseApi>>(
      `lotes/${id}`,
    );

    return mapearRespuesta(obtenerDatos(respuesta));
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
