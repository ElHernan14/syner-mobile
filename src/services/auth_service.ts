import { crear } from "@/services/ajax_service";

import type { LoginRequest, LoginResponse } from "@/models/auth";

export interface RefreshTokenRequest {
  refreshToken: string;
}

export async function iniciar_sesion(
  request: LoginRequest,
): Promise<LoginResponse> {
  return crear<LoginResponse>("auth/login", request);
}

export async function refrescar_sesion(
  request: RefreshTokenRequest,
): Promise<LoginResponse> {
  return crear<LoginResponse>("auth/refresh", request);
}
