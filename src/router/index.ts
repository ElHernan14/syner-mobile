import { createRouter, createWebHistory } from "@ionic/vue-router";

import MainLayout from "@/layouts/main_layout.vue";

import { navegacion } from "@/config/navegacion";

import LoginPage from "@/views/LoginPage.vue";

import { useAuthStore } from "@/stores/auth_store";

const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    component: LoginPage,
    name: "login",
    meta: {
      publico: true,
    },
  },
  {
    path: "/app",
    component: MainLayout,
    meta: {
      requiere_autenticacion: true,
    },
    children: navegacion.map((item) => ({
      path: item.ruta.replace("/app/", ""),
      component: item.componente,
      name: item.id,
      meta: {
        requiere_autenticacion: true,
        roles: item.roles,
      },
    })),
  },
  {
    path: "/:pathMatch(.*)*",
    redirect: "/login",
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach((to) => {
  const auth_store = useAuthStore();
  if (to.name === "login") {
    if (auth_store.autenticado) {
      return "/app/inicio";
    }

    return true;
  }

  if (to.meta.requiere_autenticacion) {
    if (!auth_store.autenticado) {
      return {
        name: "login",
      };
    }

    const roles = to.meta.roles as string[] | undefined;

    if (
      roles &&
      roles.length > 0 &&
      (!auth_store.rol || !roles.includes(auth_store.rol))
    ) {
      return {
        name: "inicio",
      };
    }
  }

  return true;
});

export default router;
