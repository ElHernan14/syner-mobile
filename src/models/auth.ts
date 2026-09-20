import type { Usuario } from "@/models/usuario";

export interface LoginRequest {
  correo: string;
  password: string;
}

export interface LoginResponseData {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  usuario: Usuario;
}

export interface LoginResponse {
  exito: boolean;
  mensaje: string;
  datos: LoginResponseData | null;
  errores: string[];
}
