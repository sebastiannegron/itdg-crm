import { apiFetch } from "./api-client";

export interface HealthStatusDto {
  status: string;
  timestamp: string;
}

export async function getHealthStatus(): Promise<HealthStatusDto> {
  return apiFetch<HealthStatusDto>("/api/v1/Health");
}
