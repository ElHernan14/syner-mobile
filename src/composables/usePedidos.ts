import { storeToRefs } from "pinia";

import { usePedidosStore } from "@/stores/pedidos_store";

export function usePedidos() {
  const pedidos_store = usePedidosStore();

  const { pedidos, cargando, error, hay_pedidos } = storeToRefs(pedidos_store);

  function cargar_pedidos() {
    return pedidos_store.cargar();
  }

  return {
    pedidos,
    cargando,
    error,
    hay_pedidos,
    cargar_pedidos,
  };
}
