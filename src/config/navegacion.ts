import type { Component } from "vue";

export interface GrupoMenu {
  id: string;
  titulo: string;
  orden: number;
}

export interface ItemNavegacion {
  id: string;
  titulo: string;
  ruta: string;
  icono: string;
  grupo_menu: string;
  orden: number;
  componente: () => Promise<{ default: Component }>;
}

const grupos_menu: GrupoMenu[] = [
  {
    id: "operacion",
    titulo: "Operación",
    orden: 1,
  },
  {
    id: "configuracion",
    titulo: "Configuración",
    orden: 2,
  },
];

const navegacion: ItemNavegacion[] = [
  {
    id: "inicio",
    titulo: "Inicio",
    ruta: "/app/inicio",
    icono: "home-outline",
    grupo_menu: "operacion",
    orden: 1,
    componente: () => import("@/views/InicioPage.vue"),
  },
  {
    id: "lotes",
    titulo: "Lotes",
    ruta: "/app/lotes",
    icono: "layers-outline",
    grupo_menu: "operacion",
    orden: 2,
    componente: () => import("@/views/LotesPage.vue"),
  },
  {
    id: "pedidos",
    titulo: "Pedidos",
    ruta: "/app/pedidos",
    icono: "receipt-outline",
    grupo_menu: "operacion",
    orden: 3,
    componente: () => import("@/views/PedidosPage.vue"),
  },
  {
    id: "proveedores",
    titulo: "Proveedores",
    ruta: "/app/proveedores",
    icono: "business-outline",
    grupo_menu: "configuracion",
    orden: 4,
    componente: () => import("@/views/ProveedoresPage.vue"),
  },
  {
    id: "usuarios",
    titulo: "Usuarios",
    ruta: "/app/usuarios",
    icono: "people-outline",
    grupo_menu: "configuracion",
    orden: 5,
    componente: () => import("@/views/UsuariosPage.vue"),
  },
];

function obtener_grupos_menu(): GrupoMenu[] {
  return [...grupos_menu].sort((a, b) => a.orden - b.orden);
}

function obtener_tabs(): ItemNavegacion[] {
  return navegacion
    .filter((item) => item.grupo_menu === "operacion")
    .sort((a, b) => a.orden - b.orden);
}

export { grupos_menu, navegacion, obtener_grupos_menu, obtener_tabs };
