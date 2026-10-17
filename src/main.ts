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

const app = createApp(App);
const pinia = createPinia();

app.use(IonicVue);
app.use(pinia);
app.use(router);

iniciar_sincronizacion_offline(pinia);

app.mount("#app");
