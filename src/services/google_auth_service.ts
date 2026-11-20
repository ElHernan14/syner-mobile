import { registerPlugin } from "@capacitor/core";
import { crear } from "@/services/ajax_service";

interface GoogleAuthPlugin {
  signIn(): Promise<{ idToken: string }>;
}
const GoogleAuth = registerPlugin<GoogleAuthPlugin>("GoogleAuth");

export async function iniciar_sesion_google() {
  const { idToken } = await GoogleAuth.signIn();
  return crear<import("@/models/auth").LoginResponse>("auth/google", { id_token: idToken });
}
