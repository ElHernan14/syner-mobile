export type EstadoPedido =
  | "fondeando"
  | "completado"
  | "comprado"
  | "enviado"
  | "entregado"
  | "cancelado";

export interface Pedido {
  id: string;
  estado: string;
  numeroSeguimiento?: string;
  codigoEntrega?: string;

  usuario: {
    id: string;
    nombre: string;
    correo: string;
  };

  lote: {
    id: string;
    nombre: string;
    categoria: string;
    precioCupo: number;
  };
}

// const pedidos: Pedido[] = [
//   {
//     id: "pedido-001",
//     usuarioId: "usuario-001",
//     loteId: "lote-001",
//     estado: "enviado",
//     numeroSeguimiento: "SYNER-000001",
//     codigoEntrega: "482913",
//   },
//   {
//     id: "pedido-002",
//     usuarioId: "usuario-003",
//     loteId: "lote-002",
//     estado: "comprado",
//     numeroSeguimiento: "SYNER-000002",
//   },
//   {
//     id: "pedido-003",
//     usuarioId: null,
//     loteId: "lote-003",
//     estado: "fondeando",
//   },
//   {
//     id: "pedido-004",
//     usuarioId: "usuario-002",
//     loteId: "lote-001",
//     estado: "entregado",
//     numeroSeguimiento: "SYNER-000004",
//     codigoEntrega: "739214",
//   },
// ];

// function obtener_pedidos(): Promise<Pedido[]> {
//   return new Promise((resolve) => {
//     setTimeout(() => {
//       resolve(pedidos);
//     }, 600);
//   });
// }

// export { pedidos, obtener_pedidos };
