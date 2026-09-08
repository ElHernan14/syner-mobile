export type EstadoUsuario = "pendiente" | "verificado" | "bloqueado";

export type RolUsuario = "usuario" | "admin";

export interface Usuario {
  id: string;
  nombre: string;
  correo: string;
  telefono?: string;
  dni: string;
  rol: RolUsuario;
  estado: EstadoUsuario;
  avatar?: string | null;
  direccion?: string;
}
