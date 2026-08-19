import { createRouter, createWebHistory } from "@ionic/vue-router";

import MainLayout from "@/layouts/main_layout.vue";
import { navegacion } from "@/config/navegacion";

const routes = [
  {
    path: "/",
    redirect: "/app/inicio",
  },
  {
    path: "/app",
    component: MainLayout,
    children: navegacion.map((item) => ({
      path: item.ruta.replace("/app/", ""),
      component: item.componente,
      name: item.id,
    })),
  },
  {
    path: "/:pathMatch(.*)*",
    redirect: "/app/inicio",
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;
