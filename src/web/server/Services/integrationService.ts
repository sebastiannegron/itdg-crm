import { apiFetch } from "./api-client";

export interface GoogleConnectionStatusDto {
  is_connected: boolean;
  connected_at: string | null;
}

export async function getGoogleConnectionStatus(): Promise<GoogleConnectionStatusDto> {
  return apiFetch<GoogleConnectionStatusDto>(
    "/api/v1/Integrations/Google/Status",
  );
}

export async function disconnectGoogle(): Promise<void> {
  return apiFetch<void>("/api/v1/Integrations/Google", {
    method: "DELETE",
  });
}

export function getGoogleAuthUrl(): string {
  const apiBase = process.env.API_BASE_URL ?? "http://localhost:5000";
  return `${apiBase}/api/v1/Integrations/Google/Auth`;
}

export interface GmailConnectionStatusDto {
  is_connected: boolean;
  connected_at: string | null;
}

export async function getGmailConnectionStatus(): Promise<GmailConnectionStatusDto> {
  return apiFetch<GmailConnectionStatusDto>(
    "/api/v1/Integrations/Gmail/Status",
  );
}

export async function disconnectGmail(): Promise<void> {
  return apiFetch<void>("/api/v1/Integrations/Gmail", {
    method: "DELETE",
  });
}

export function getGmailAuthUrl(): string {
  const apiBase = process.env.API_BASE_URL ?? "http://localhost:5000";
  return `${apiBase}/api/v1/Integrations/Gmail/Auth`;
}
