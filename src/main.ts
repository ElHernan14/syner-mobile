import { createApp } from "vue";
import { createPinia } from "pinia";
import App from "./App.vue";
import router from "./router";

import "@ionic/vue/css/core.css";
import "@ionic/vue/css/normalize.css";
import "@ionic/vue/css/structure.css";
import "@ionic/vue/css/typography.css";
import "@ionic/vue/css/padding.css";
import "@ionic/vue/css/float-elements.css";
import "@ionic/vue/css/text-alignment.css";
import "@ionic/vue/css/text-transformation.css";
import "@ionic/vue/css/flex-utils.css";
import "@ionic/vue/css/display.css";

import "./theme/variables.css";

import { IonicVue } from "@ionic/vue";
import { iniciar_sincronizacion_offline } from "@/services/sincronizacion_offline_service";
import { App as CapacitorApp } from "@capacitor/app";

const app = createApp(App);
const pinia = createPinia();

app.use(IonicVue);
app.use(pinia);
app.use(router);

iniciar_sincronizacion_offline(pinia);

app.mount("#app");

function abrir_enlace_publico(url: string) {
  try {
    const ruta = new URL(url).pathname;
    if (/^\/p\/[a-f0-9]{32}$/i.test(ruta)) void router.replace(ruta);
  } catch { /* Se ignoran URLs ajenas al App Link. */ }
}

void CapacitorApp.addListener("appUrlOpen", ({ url }) => abrir_enlace_publico(url));
void CapacitorApp.getLaunchUrl().then((launch) => { if (launch?.url) abrir_enlace_publico(launch.url); });
