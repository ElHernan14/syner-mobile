import { storeToRefs } from "pinia";

import { usePedidosStore } from "@/stores/pedidos_store";

export function usePedidos() {
  const pedidos_store = usePedidosStore();

  const { pedidos, cargando, error, hay_pedidos } = storeToRefs(pedidos_store);

  function cargar_pedidos(pedido_id?: number) {
    return pedidos_store.cargar(1, pedido_id);
  }

  return {
    pedidos,
    cargando,
    error,
    hay_pedidos,
    cargar_pedidos,
  };
}
