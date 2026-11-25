export interface Direccion {
  id: number;
  calle: string;
  numero: string;
  ciudad: string;
  provincia: string;
  codigoPostal: string;
  referencia: string | null;
  latitud: number;
  longitud: number;
  esPrincipal: boolean;
  fechaCreacion: string;
  fechaActualizacion: string;
}

export interface GuardarDireccionRequest {
  calle: string;
  numero: string;
  ciudad: string;
  provincia: string;
  codigoPostal: string;
  referencia: string | null;
  latitud: number;
  longitud: number;
}
