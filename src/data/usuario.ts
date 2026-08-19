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

const usuarios: Usuario[] = [
  {
    id: "usuario-001",
    nombre: "Juan Pérez",
    correo: "juan@example.com",
    telefono: "+54 9 266 000000",
    dni: "40123456",
    rol: "usuario",
    estado: "verificado",
    avatar: null,
    direccion: "Av. Illia 123, San Luis",
  },
  {
    id: "usuario-002",
    nombre: "María González",
    correo: "maria@example.com",
    telefono: "+54 9 266 111111",
    dni: "41234567",
    rol: "usuario",
    estado: "pendiente",
    avatar: null,
  },
  {
    id: "usuario-003",
    nombre: "Carlos Rodríguez",
    correo: "carlos@example.com",
    dni: "39876543",
    rol: "usuario",
    estado: "verificado",
    avatar: null,
    direccion: "Rivadavia 456, San Luis",
  },
  {
    id: "usuario-004",
    nombre: "Admin SYNER",
    correo: "admin@syner.demo",
    dni: "38000111",
    rol: "admin",
    estado: "verificado",
    avatar: null,
    direccion: "Oficina SYNER",
  },
];

function obtener_usuarios(): Promise<Usuario[]> {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(usuarios);
    }, 600);
  });
}

export { usuarios, obtener_usuarios };
