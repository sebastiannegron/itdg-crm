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

export interface CalendarConnectionStatusDto {
  is_connected: boolean;
  connected_at: string | null;
}

export async function getCalendarConnectionStatus(): Promise<CalendarConnectionStatusDto> {
  return apiFetch<CalendarConnectionStatusDto>(
    "/api/v1/Integrations/Calendar/Status",
  );
}

export interface MsGraphConnectionStatusDto {
  is_configured: boolean;
}

export async function getMsGraphConnectionStatus(): Promise<MsGraphConnectionStatusDto> {
  return apiFetch<MsGraphConnectionStatusDto>(
    "/api/v1/Integrations/MsGraph/Status",
  );
}

export interface AzureOpenAiConnectionStatusDto {
  is_configured: boolean;
}

export async function getAzureOpenAiConnectionStatus(): Promise<AzureOpenAiConnectionStatusDto> {
  return apiFetch<AzureOpenAiConnectionStatusDto>(
    "/api/v1/Integrations/AzureOpenAi/Status",
  );
}
